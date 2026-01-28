"use client";

import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { toast } from "sonner";
import { api, Job } from "@/lib/api";
import { jobsHub, JobUpdate } from "@/lib/signalr";

type ToolType = "talking-head" | "virtual-tryon" | "image-to-video" | "motion-transfer" | "face-swap";

const tools = [
    { id: "talking-head", label: "Talking Head", icon: "🎭", description: "Tạo video nhân vật nói từ ảnh + audio", jobType: "TalkingHead" },
    { id: "virtual-tryon", label: "Thử Đồ Ảo", icon: "👕", description: "Mặc quần áo lên ảnh mẫu", jobType: "VirtualTryOn" },
    { id: "image-to-video", label: "Ảnh → Video", icon: "🎬", description: "Biến ảnh tĩnh thành video 4 giây", jobType: "ImageToVideo" },
    { id: "motion-transfer", label: "Chuyển Động", icon: "💃", description: "Chuyển động từ video mẫu", jobType: "MotionTransfer" },
    { id: "face-swap", label: "Đổi Mặt", icon: "🎭", description: "Đổi mặt trong video", jobType: "FaceSwap" },
];

export default function StudioPage() {
    const [selectedTool, setSelectedTool] = useState<ToolType>("talking-head");
    const [currentJob, setCurrentJob] = useState<Job | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [files, setFiles] = useState<Record<string, File>>({});

    // Listen for real-time job updates
    useEffect(() => {
        if (currentJob?.id) {
            const unsubscribe = jobsHub.onJobUpdate(currentJob.id, (update: JobUpdate) => {
                setCurrentJob((prev) => {
                    if (!prev) return prev;
                    return {
                        ...prev,
                        status: update.status as Job["status"], // Ensure type compatibility
                        outputUrl: update.outputUrl,
                        errorMessage: update.error
                    };
                });

                const statusUpper = update.status.toUpperCase();
                if (statusUpper === "COMPLETED") {
                    toast.success("🎉 Video đã sẵn sàng!");
                } else if (statusUpper === "FAILED") {
                    toast.error(`❌ Lỗi: ${update.error || "Không xác định"}`);
                }
            });

            return () => unsubscribe();
        }
    }, [currentJob?.id]);

    // Connect to SignalR when component mounts
    useEffect(() => {
        const token = api.getToken();
        if (token) {
            jobsHub.connect(token).catch((err) => {
                console.warn("SignalR connection failed:", err);
            });
        }

        return () => {
            jobsHub.disconnect();
        };
    }, []);

    const handleFileChange = (name: string, file: File | null) => {
        if (file) {
            setFiles((prev) => ({ ...prev, [name]: file }));
        } else {
            setFiles((prev) => {
                const newFiles = { ...prev };
                delete newFiles[name];
                return newFiles;
            });
        }
    };

    const handleSubmitJob = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData(e.currentTarget); // Initialize synchronous data immediately
        setIsSubmitting(true);

        try {
            const tool = tools.find((t) => t.id === selectedTool);
            if (!tool) throw new Error("Invalid tool selected");

            // Upload files first
            const uploadedUrls: Record<string, string> = {};

            for (const [name, file] of Object.entries(files)) {
                toast.loading(`Đang upload ${file.name}...`, { id: `upload-${name}` });
                const result = await api.uploadFile(file);
                toast.dismiss(`upload-${name}`);

                if (result.data?.url) {
                    uploadedUrls[name] = result.data.url;
                } else {
                    throw new Error(`Upload failed for ${file.name}`);
                }
            }

            // Create job with uploaded file URLs
            const jobRequest = {
                jobType: tool.jobType,
                sourceImageUrl: uploadedUrls["sourceImage"] || uploadedUrls["modelImage"],
                audioUrl: uploadedUrls["audio"],
                garmentImageUrl: uploadedUrls["garmentImage"],
                skeletonVideoUrl: uploadedUrls["skeletonVideo"] || uploadedUrls["sourceVideo"],
                targetFaceUrl: uploadedUrls["targetFace"],
                outputResolution: formData.get("resolution")?.toString() || "720p",
                priority: "normal",
            };

            toast.loading("Đang tạo công việc...", { id: "create-job" });
            const result = await api.createJob(jobRequest);
            toast.dismiss("create-job");

            if (result.data) {
                setCurrentJob(result.data);
                toast.success("✅ Đã gửi công việc!");
                setFiles({});
            } else {
                throw new Error(result.error || "Failed to create job");
            }
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Có lỗi xảy ra");
        } finally {
            setIsSubmitting(false);
        }
    };

    // Poll for job status if SignalR is not connected
    useEffect(() => {
        if (currentJob?.id && !jobsHub.isConnected) {
            const interval = setInterval(async () => {
                const result = await api.getJob(currentJob.id);
                if (result.data) {
                    setCurrentJob(result.data);
                    if (result.data.status === "Completed" || result.data.status === "Failed") {
                        clearInterval(interval);
                        if (result.data.status === "Completed") {
                            toast.success("🎉 Video đã sẵn sàng!");
                        }
                    }
                }
            }, 3000);

            return () => clearInterval(interval);
        }
    }, [currentJob?.id]);

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-white">AI Studio</h1>
                <p className="text-gray-400">Chọn công cụ AI và tạo video của bạn</p>
            </div>

            <Tabs value={selectedTool} onValueChange={(v) => { setSelectedTool(v as ToolType); setFiles({}); setCurrentJob(null); }}>
                <TabsList className="grid grid-cols-5 bg-slate-800 p-1">
                    {tools.map((tool) => (
                        <TabsTrigger key={tool.id} value={tool.id} className="data-[state=active]:bg-purple-600">
                            <span className="mr-2">{tool.icon}</span>
                            <span className="hidden md:inline">{tool.label}</span>
                        </TabsTrigger>
                    ))}
                </TabsList>

                {/* Talking Head */}
                <TabsContent value="talking-head">
                    <ToolCard
                        title="Talking Head"
                        description="Tạo video nhân vật nói từ ảnh chân dung + file âm thanh"
                        onSubmit={handleSubmitJob}
                        currentJob={currentJob}
                        isSubmitting={isSubmitting}
                    >
                        <UploadField label="Ảnh chân dung" name="sourceImage" accept="image/*" onChange={handleFileChange} />
                        <UploadField label="File âm thanh" name="audio" accept="audio/*" onChange={handleFileChange} />
                        <SelectField label="Độ phân giải" name="resolution" options={["720p", "1080p"]} />
                    </ToolCard>
                </TabsContent>

                {/* Virtual Try-On */}
                <TabsContent value="virtual-tryon">
                    <ToolCard
                        title="Thử Đồ Ảo"
                        description="Upload ảnh người mẫu và ảnh quần áo để ghép"
                        onSubmit={handleSubmitJob}
                        currentJob={currentJob}
                        isSubmitting={isSubmitting}
                    >
                        <UploadField label="Ảnh người mẫu" name="modelImage" accept="image/*" onChange={handleFileChange} />
                        <UploadField label="Ảnh quần áo" name="garmentImage" accept="image/*" onChange={handleFileChange} />
                        <SelectField label="Loại" name="category" options={["Áo", "Quần", "Váy"]} />
                    </ToolCard>
                </TabsContent>

                {/* Image to Video */}
                <TabsContent value="image-to-video">
                    <ToolCard
                        title="Ảnh → Video"
                        description="Biến ảnh tĩnh thành video động 4 giây"
                        onSubmit={handleSubmitJob}
                        currentJob={currentJob}
                        isSubmitting={isSubmitting}
                    >
                        <UploadField label="Ảnh nguồn" name="sourceImage" accept="image/*" onChange={handleFileChange} />
                        <SelectField label="Độ phân giải" name="resolution" options={["576p", "720p", "1080p"]} />
                        <RangeField label="Mức độ chuyển động" name="motionBucket" min={1} max={255} defaultValue={127} />
                    </ToolCard>
                </TabsContent>

                {/* Motion Transfer */}
                <TabsContent value="motion-transfer">
                    <ToolCard
                        title="Chuyển Động"
                        description="Chuyển chuyển động từ video mẫu sang ảnh"
                        onSubmit={handleSubmitJob}
                        currentJob={currentJob}
                        isSubmitting={isSubmitting}
                    >
                        <UploadField label="Ảnh nguồn" name="sourceImage" accept="image/*" onChange={handleFileChange} />
                        <UploadField label="Video chuyển động" name="skeletonVideo" accept="video/*" onChange={handleFileChange} />
                    </ToolCard>
                </TabsContent>

                {/* Face Swap */}
                <TabsContent value="face-swap">
                    <ToolCard
                        title="Đổi Mặt"
                        description="Đổi mặt trong video với khuôn mặt khác"
                        onSubmit={handleSubmitJob}
                        currentJob={currentJob}
                        isSubmitting={isSubmitting}
                    >
                        <UploadField label="Video nguồn" name="sourceVideo" accept="video/*" onChange={handleFileChange} />
                        <UploadField label="Ảnh khuôn mặt mới" name="targetFace" accept="image/*" onChange={handleFileChange} />
                    </ToolCard>
                </TabsContent>
            </Tabs>
        </div>
    );
}

// Tool Card Component
function ToolCard({
    title,
    description,
    children,
    onSubmit,
    currentJob,
    isSubmitting,
}: {
    title: string;
    description: string;
    children: React.ReactNode;
    onSubmit: (e: React.FormEvent<HTMLFormElement>) => void;
    currentJob: Job | null;
    isSubmitting: boolean;
}) {
    const statusUpper = currentJob?.status?.toUpperCase();
    const isProcessing = Boolean(currentJob && !["COMPLETED", "FAILED"].includes(statusUpper || ""));

    let progress = 0;
    if (statusUpper === "QUEUED") progress = 25;
    else if (statusUpper === "PROCESSING") progress = 60;
    else if (statusUpper === "COMPLETED") progress = 100;

    return (
        <div className="grid md:grid-cols-2 gap-6 mt-6">
            {/* Input Form */}
            <Card className="bg-slate-900 border-slate-800">
                <CardHeader>
                    <CardTitle className="text-white">{title}</CardTitle>
                    <CardDescription>{description}</CardDescription>
                </CardHeader>
                <CardContent>
                    <form onSubmit={onSubmit} className="space-y-4">
                        {children}
                        <Button
                            type="submit"
                            className="w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
                            disabled={isSubmitting || isProcessing}
                        >
                            {isSubmitting ? "Đang upload..." : isProcessing ? "Đang xử lý..." : "🚀 Tạo Video"}
                        </Button>
                    </form>
                </CardContent>
            </Card>

            {/* Preview / Result */}
            <Card className="bg-slate-900 border-slate-800">
                <CardHeader>
                    <CardTitle className="text-white">Kết quả</CardTitle>
                </CardHeader>
                <CardContent>
                    {currentJob ? (
                        <div className="space-y-4">
                            <div className="flex items-center justify-between">
                                <span className="text-gray-400">Trạng thái:</span>
                                <Badge variant={
                                    statusUpper === "COMPLETED" ? "default" :
                                        statusUpper === "FAILED" ? "destructive" : "secondary"
                                }>
                                    {statusUpper === "PENDING" && "📋 Đang chờ"}
                                    {statusUpper === "QUEUED" && "⏳ Trong hàng đợi"}
                                    {statusUpper === "PROCESSING" && "⚙️ Đang xử lý"}
                                    {statusUpper === "COMPLETED" && "✅ Hoàn thành"}
                                    {statusUpper === "FAILED" && "❌ Thất bại"}
                                    {!["PENDING", "QUEUED", "PROCESSING", "COMPLETED", "FAILED"].includes(statusUpper || "") && currentJob.status}
                                </Badge>
                            </div>
                            <Progress value={progress} className="h-2" />

                            {statusUpper === "COMPLETED" && currentJob.outputUrl && (
                                <div className="space-y-4">
                                    <div className="aspect-video bg-slate-800 rounded-lg overflow-hidden">
                                        {currentJob.outputUrl.endsWith(".mp4") ? (
                                            <video src={currentJob.outputUrl} controls className="w-full h-full object-contain" />
                                        ) : (
                                            <img src={currentJob.outputUrl} alt="Output" className="w-full h-full object-contain" />
                                        )}
                                    </div>
                                    <a href={currentJob.outputUrl} target="_blank" rel="noopener noreferrer" download>
                                        <Button className="w-full">📥 Tải Video</Button>
                                    </a>
                                </div>
                            )}

                            {statusUpper === "FAILED" && (
                                <div className="p-4 bg-red-500/10 border border-red-500/30 rounded-lg">
                                    <p className="text-red-400">{currentJob.errorMessage || "Có lỗi xảy ra khi xử lý"}</p>
                                </div>
                            )}

                            {isProcessing && (
                                <p className="text-sm text-gray-400 text-center animate-pulse">
                                    ⏳ Đang xử lý... Có thể mất 30-120 giây
                                </p>
                            )}
                        </div>
                    ) : (
                        <div className="aspect-video bg-slate-800 rounded-lg flex items-center justify-center">
                            <p className="text-gray-500">Kết quả sẽ hiển thị ở đây</p>
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}

// Upload Field Component
function UploadField({
    label,
    name,
    accept,
    onChange,
}: {
    label: string;
    name: string;
    accept: string;
    onChange: (name: string, file: File | null) => void;
}) {
    return (
        <div className="space-y-2">
            <label className="text-sm text-gray-300">{label}</label>
            <Input
                type="file"
                name={name}
                accept={accept}
                className="bg-slate-800 border-slate-700 text-white file:bg-purple-600 file:text-white file:border-0 file:mr-4"
                onChange={(e) => onChange(name, e.target.files?.[0] || null)}
            />
        </div>
    );
}

// Select Field Component
function SelectField({ label, name, options }: { label: string; name: string; options: string[] }) {
    return (
        <div className="space-y-2">
            <label className="text-sm text-gray-300">{label}</label>
            <select name={name} className="w-full bg-slate-800 border border-slate-700 rounded-md p-2 text-white">
                {options.map((opt) => (
                    <option key={opt} value={opt}>{opt}</option>
                ))}
            </select>
        </div>
    );
}

// Range Field Component
function RangeField({ label, name, min, max, defaultValue }: { label: string; name: string; min: number; max: number; defaultValue: number }) {
    return (
        <div className="space-y-2">
            <label className="text-sm text-gray-300">{label}</label>
            <input
                type="range"
                name={name}
                min={min}
                max={max}
                defaultValue={defaultValue}
                className="w-full"
            />
        </div>
    );
}

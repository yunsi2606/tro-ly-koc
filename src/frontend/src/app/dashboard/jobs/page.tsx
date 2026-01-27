"use client";

import { useEffect, useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { api, Job } from "@/lib/api";
import { toast } from "sonner";

const statusConfig = {
    Pending: { label: "Chờ xử lý", icon: "📋", variant: "outline" as const },
    Queued: { label: "Đang chờ", icon: "⏳", variant: "secondary" as const },
    Processing: { label: "Đang xử lý", icon: "⚙️", variant: "secondary" as const },
    Completed: { label: "Hoàn thành", icon: "✅", variant: "default" as const },
    Failed: { label: "Thất bại", icon: "❌", variant: "destructive" as const },
};

const typeConfig: Record<string, string> = {
    TalkingHead: "🎭",
    VirtualTryOn: "👕",
    ImageToVideo: "🎬",
    MotionTransfer: "💃",
    FaceSwap: "🎭",
};

export default function JobsPage() {
    const [jobs, setJobs] = useState<Job[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const fetchJobs = async () => {
        setIsLoading(true);
        try {
            const result = await api.getJobs();
            if (result.data) {
                setJobs(result.data);
            }
        } catch (error) {
            toast.error("Lỗi tải danh sách công việc");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchJobs();
    }, []);

    const formatDuration = (ms?: number) => {
        if (!ms) return "-";
        if (ms < 1000) return `${ms}ms`;
        if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`;
        return `${Math.floor(ms / 60000)}m ${Math.round((ms % 60000) / 1000)}s`;
    };

    if (isLoading) {
        return (
            <div className="space-y-6">
                <div className="flex items-center justify-between">
                    <Skeleton className="h-8 w-48 bg-slate-800" />
                    <Skeleton className="h-10 w-24 bg-slate-800" />
                </div>
                <Skeleton className="h-96 bg-slate-800" />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-white">Lịch Sử Công Việc</h1>
                    <p className="text-gray-400">Tất cả các job render của bạn ({jobs.length} jobs)</p>
                </div>
                <Button variant="outline" onClick={fetchJobs}>🔄 Làm mới</Button>
            </div>

            <Card className="bg-slate-900 border-slate-800">
                <CardContent className="p-0">
                    {jobs.length === 0 ? (
                        <div className="p-8 text-center text-gray-400">
                            <p className="text-4xl mb-4">📭</p>
                            <p>Chưa có công việc nào</p>
                        </div>
                    ) : (
                        <table className="w-full">
                            <thead className="bg-slate-800">
                                <tr>
                                    <th className="text-left p-4 text-gray-400 font-medium">Job ID</th>
                                    <th className="text-left p-4 text-gray-400 font-medium">Loại</th>
                                    <th className="text-left p-4 text-gray-400 font-medium">Trạng thái</th>
                                    <th className="text-left p-4 text-gray-400 font-medium">Thời gian</th>
                                    <th className="text-left p-4 text-gray-400 font-medium">Tạo lúc</th>
                                    <th className="text-left p-4 text-gray-400 font-medium">Hành động</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-800">
                                {jobs.map((job) => {
                                    const status = statusConfig[job.status as keyof typeof statusConfig] || statusConfig.Pending;
                                    return (
                                        <tr key={job.id} className="hover:bg-slate-800/50">
                                            <td className="p-4 text-white font-mono text-sm">{job.id.slice(0, 8)}...</td>
                                            <td className="p-4">
                                                <span className="flex items-center gap-2 text-white">
                                                    <span>{typeConfig[job.jobType] || "🎬"}</span>
                                                    {job.jobType}
                                                </span>
                                            </td>
                                            <td className="p-4">
                                                <Badge variant={status.variant}>
                                                    {status.icon} {status.label}
                                                </Badge>
                                            </td>
                                            <td className="p-4 text-gray-400">{formatDuration(job.processingTimeMs)}</td>
                                            <td className="p-4 text-gray-400">
                                                {new Date(job.createdAt).toLocaleString("vi-VN")}
                                            </td>
                                            <td className="p-4">
                                                {job.outputUrl ? (
                                                    <a href={job.outputUrl} target="_blank" rel="noopener noreferrer">
                                                        <Button variant="ghost" size="sm">📥 Tải về</Button>
                                                    </a>
                                                ) : job.status === "Failed" ? (
                                                    <Button variant="ghost" size="sm">🔄 Thử lại</Button>
                                                ) : (
                                                    <span className="text-gray-500">-</span>
                                                )}
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}

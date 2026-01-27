"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { api, Job, Wallet, Subscription } from "@/lib/api";

const quickActions = [
    { label: "Tạo Talking Head", href: "/dashboard/studio?tool=talking-head", icon: "🎭", color: "from-purple-600 to-pink-600" },
    { label: "Thử Đồ Ảo", href: "/dashboard/studio?tool=virtual-tryon", icon: "👕", color: "from-blue-600 to-cyan-600" },
    { label: "Ảnh thành Video", href: "/dashboard/studio?tool=image-to-video", icon: "🎬", color: "from-green-600 to-emerald-600" },
    { label: "Đổi Mặt", href: "/dashboard/studio?tool=face-swap", icon: "🎭", color: "from-orange-600 to-red-600" },
];

const typeConfig: Record<string, string> = {
    TalkingHead: "🎭",
    VirtualTryOn: "👕",
    ImageToVideo: "🎬",
    MotionTransfer: "💃",
    FaceSwap: "🎭",
};

export default function DashboardPage() {
    const [isLoading, setIsLoading] = useState(true);
    const [wallet, setWallet] = useState<Wallet | null>(null);
    const [subscription, setSubscription] = useState<Subscription | null>(null);
    const [recentJobs, setRecentJobs] = useState<Job[]>([]);

    useEffect(() => {
        async function fetchData() {
            setIsLoading(true);
            try {
                const [walletRes, subRes, jobsRes] = await Promise.all([
                    api.getWallet(),
                    api.getCurrentSubscription(),
                    api.getJobs(),
                ]);

                if (walletRes.data) setWallet(walletRes.data);
                if (subRes.data) setSubscription(subRes.data);
                if (jobsRes.data) setRecentJobs(jobsRes.data.slice(0, 5));
            } catch (error) {
                console.error("Error fetching dashboard data:", error);
            } finally {
                setIsLoading(false);
            }
        }

        fetchData();
    }, []);

    if (isLoading) {
        return <DashboardSkeleton />;
    }

    return (
        <div className="space-y-6">
            {/* Quick Actions */}
            <div>
                <h2 className="text-lg font-semibold text-white mb-4">Tạo Nhanh</h2>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    {quickActions.map((action, index) => (
                        <Link key={index} href={action.href}>
                            <Card className={`bg-gradient-to-br ${action.color} border-0 hover:opacity-90 transition-opacity cursor-pointer`}>
                                <CardContent className="p-4 text-center">
                                    <span className="text-3xl block mb-2">{action.icon}</span>
                                    <p className="text-white font-medium">{action.label}</p>
                                </CardContent>
                            </Card>
                        </Link>
                    ))}
                </div>
            </div>

            {/* Stats Grid */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <Card className="bg-slate-900 border-slate-800">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <span className="text-2xl">🎬</span>
                            <div>
                                <p className="text-sm text-gray-400">Lượt render còn lại</p>
                                <p className="text-xl font-bold text-white">
                                    {subscription?.remainingJobs ?? 0}
                                </p>
                            </div>
                        </div>
                        <Progress value={((subscription?.remainingJobs ?? 0) / 10) * 100} className="mt-3 h-1" />
                    </CardContent>
                </Card>

                <Card className="bg-slate-900 border-slate-800">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <span className="text-2xl">💳</span>
                            <div>
                                <p className="text-sm text-gray-400">Số dư ví</p>
                                <p className="text-xl font-bold text-white">
                                    {wallet?.balance?.toLocaleString() ?? 0}
                                    <span className="text-sm text-gray-400 ml-1">VNĐ</span>
                                </p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="bg-slate-900 border-slate-800">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <span className="text-2xl">📹</span>
                            <div>
                                <p className="text-sm text-gray-400">Video đã tạo</p>
                                <p className="text-xl font-bold text-white">
                                    {recentJobs.filter((j) => j.status === "Completed").length}
                                </p>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="bg-slate-900 border-slate-800">
                    <CardContent className="p-4">
                        <div className="flex items-center gap-3">
                            <span className="text-2xl">⭐</span>
                            <div>
                                <p className="text-sm text-gray-400">Gói hiện tại</p>
                                <p className="text-xl font-bold text-white">
                                    {subscription?.tierName ?? "Free"}
                                </p>
                            </div>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Recent Jobs */}
            <Card className="bg-slate-900 border-slate-800">
                <CardHeader className="flex flex-row items-center justify-between">
                    <div>
                        <CardTitle className="text-white">Công Việc Gần Đây</CardTitle>
                        <CardDescription>Danh sách các job render gần nhất</CardDescription>
                    </div>
                    <Link href="/dashboard/jobs">
                        <Button variant="outline" size="sm">Xem tất cả</Button>
                    </Link>
                </CardHeader>
                <CardContent>
                    {recentJobs.length === 0 ? (
                        <p className="text-gray-400 text-center py-8">Chưa có công việc nào</p>
                    ) : (
                        <div className="space-y-3">
                            {recentJobs.map((job) => (
                                <div key={job.id} className="flex items-center justify-between p-3 bg-slate-800 rounded-lg">
                                    <div className="flex items-center gap-3">
                                        <Badge variant={
                                            job.status === "Completed" ? "default" :
                                                job.status === "Processing" ? "secondary" :
                                                    job.status === "Failed" ? "destructive" : "outline"
                                        }>
                                            {job.status === "Completed" ? "✅" :
                                                job.status === "Processing" ? "⏳" :
                                                    job.status === "Failed" ? "❌" : "📋"}
                                        </Badge>
                                        <div>
                                            <p className="text-white font-medium flex items-center gap-2">
                                                <span>{typeConfig[job.jobType] || "🎬"}</span>
                                                {job.jobType}
                                            </p>
                                            <p className="text-sm text-gray-400">
                                                {new Date(job.createdAt).toLocaleString("vi-VN")}
                                            </p>
                                        </div>
                                    </div>
                                    {job.outputUrl && (
                                        <a href={job.outputUrl} target="_blank" rel="noopener noreferrer">
                                            <Button variant="ghost" size="sm">📥 Tải về</Button>
                                        </a>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}

function DashboardSkeleton() {
    return (
        <div className="space-y-6">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {[1, 2, 3, 4].map((i) => (
                    <Skeleton key={i} className="h-24 bg-slate-800" />
                ))}
            </div>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {[1, 2, 3, 4].map((i) => (
                    <Skeleton key={i} className="h-20 bg-slate-800" />
                ))}
            </div>
            <Skeleton className="h-64 bg-slate-800" />
        </div>
    );
}

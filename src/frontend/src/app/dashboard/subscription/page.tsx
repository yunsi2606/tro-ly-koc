"use client";

import { useEffect, useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { api, Subscription } from "@/lib/api";
import { toast } from "sonner";

const tiers = [
    {
        id: "free",
        name: "Free",
        price: 0,
        features: [
            "10 lượt render/tháng",
            "Độ phân giải 720p",
            "Có watermark",
            "Hỗ trợ qua email",
        ],
    },
    {
        id: "pro",
        name: "Pro",
        price: 199000,
        popular: true,
        features: [
            "100 lượt render/tháng",
            "Độ phân giải 1080p",
            "Không watermark",
            "Ưu tiên xử lý",
            "Hỗ trợ 24/7",
        ],
    },
    {
        id: "enterprise",
        name: "Enterprise",
        price: 999000,
        features: [
            "Không giới hạn render",
            "Độ phân giải 4K",
            "Không watermark",
            "Ưu tiên cao nhất",
            "API access",
            "Hỗ trợ riêng",
        ],
    },
];

export default function SubscriptionPage() {
    const [subscription, setSubscription] = useState<Subscription | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [actionLoading, setActionLoading] = useState<string | null>(null);

    const fetchSubscription = async () => {
        setIsLoading(true);
        try {
            const result = await api.getCurrentSubscription();
            if (result.data) {
                setSubscription(result.data);
            }
        } catch (error) {
            console.error("Error fetching subscription:", error);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchSubscription();
    }, []);

    const handleSubscribe = async (tierId: string) => {
        setActionLoading(tierId);
        try {
            const result = await api.subscribeTier(tierId);
            if (result.data) {
                setSubscription(result.data);
                toast.success(`Đã đăng ký gói ${tierId}!`);
            } else {
                toast.error(result.error || "Không thể đăng ký gói");
            }
        } catch (error) {
            toast.error("Lỗi đăng ký gói");
        } finally {
            setActionLoading(null);
        }
    };

    const handleCancel = async () => {
        setActionLoading("cancel");
        try {
            const result = await api.cancelSubscription();
            if (result.status === 200) {
                toast.success("Đã hủy gói đăng ký");
                fetchSubscription();
            } else {
                toast.error("Không thể hủy gói");
            }
        } catch (error) {
            toast.error("Lỗi hủy gói");
        } finally {
            setActionLoading(null);
        }
    };

    if (isLoading) {
        return (
            <div className="space-y-6">
                <Skeleton className="h-8 w-48 bg-slate-800" />
                <Skeleton className="h-24 bg-slate-800" />
                <div className="grid md:grid-cols-3 gap-6">
                    {[1, 2, 3].map((i) => (
                        <Skeleton key={i} className="h-80 bg-slate-800" />
                    ))}
                </div>
            </div>
        );
    }

    const currentTierId = subscription?.tierId?.toLowerCase() || "free";

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-white">Gói Đăng Ký</h1>
                <p className="text-gray-400">Chọn gói phù hợp với nhu cầu của bạn</p>
            </div>

            {/* Current Plan */}
            <Card className="bg-slate-900 border-slate-800">
                <CardContent className="p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 mb-1">Gói hiện tại</p>
                            <div className="flex items-center gap-3">
                                <span className="text-2xl font-bold text-white">
                                    {subscription?.tierName || "Free"}
                                </span>
                                <Badge variant={subscription?.status === "Active" ? "default" : "secondary"}>
                                    {subscription?.status === "Active" ? "Đang sử dụng" : subscription?.status || "Active"}
                                </Badge>
                            </div>
                            <p className="text-sm text-gray-400 mt-2">
                                Còn {subscription?.remainingJobs ?? 10} lượt render.
                                {subscription?.periodEnd && (
                                    <> Hết hạn: {new Date(subscription.periodEnd).toLocaleDateString("vi-VN")}</>
                                )}
                            </p>
                        </div>
                        {currentTierId !== "free" && (
                            <Button
                                variant="outline"
                                onClick={handleCancel}
                                disabled={actionLoading === "cancel"}
                            >
                                {actionLoading === "cancel" ? "Đang hủy..." : "Hủy gói"}
                            </Button>
                        )}
                    </div>
                </CardContent>
            </Card>

            {/* Pricing Cards */}
            <div className="grid md:grid-cols-3 gap-6">
                {tiers.map((tier) => {
                    const isCurrentPlan = currentTierId === tier.id;
                    return (
                        <Card
                            key={tier.id}
                            className={`relative ${tier.popular
                                    ? "bg-gradient-to-br from-purple-600/20 to-pink-600/20 border-purple-500"
                                    : "bg-slate-900 border-slate-800"
                                }`}
                        >
                            {tier.popular && (
                                <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                                    <Badge className="bg-gradient-to-r from-purple-600 to-pink-600">
                                        🔥 Phổ biến nhất
                                    </Badge>
                                </div>
                            )}
                            <CardHeader>
                                <CardTitle className="text-white text-xl">{tier.name}</CardTitle>
                                <CardDescription>
                                    <span className="text-3xl font-bold text-white">
                                        {tier.price === 0 ? "Miễn phí" : `${tier.price.toLocaleString()}đ`}
                                    </span>
                                    {tier.price > 0 && <span className="text-gray-400">/tháng</span>}
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                <ul className="space-y-2">
                                    {tier.features.map((feature, index) => (
                                        <li key={index} className="flex items-center gap-2 text-gray-300">
                                            <span className="text-green-400">✓</span>
                                            {feature}
                                        </li>
                                    ))}
                                </ul>
                                <Button
                                    className={`w-full ${tier.popular
                                            ? "bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
                                            : ""
                                        }`}
                                    variant={tier.popular ? "default" : "outline"}
                                    disabled={isCurrentPlan || actionLoading === tier.id}
                                    onClick={() => handleSubscribe(tier.id)}
                                >
                                    {isCurrentPlan
                                        ? "Đang sử dụng"
                                        : actionLoading === tier.id
                                            ? "Đang xử lý..."
                                            : "Chọn gói này"}
                                </Button>
                            </CardContent>
                        </Card>
                    );
                })}
            </div>

            {/* FAQ */}
            <Card className="bg-slate-900 border-slate-800">
                <CardHeader>
                    <CardTitle className="text-white">Câu Hỏi Thường Gặp</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div>
                        <p className="text-white font-medium">Tôi có thể hủy gói bất cứ lúc nào không?</p>
                        <p className="text-gray-400 text-sm">Có, bạn có thể hủy gói bất cứ lúc nào. Gói sẽ vẫn hoạt động đến hết chu kỳ thanh toán.</p>
                    </div>
                    <div>
                        <p className="text-white font-medium">Lượt render không dùng hết có được cộng dồn không?</p>
                        <p className="text-gray-400 text-sm">Không, lượt render sẽ reset vào đầu mỗi tháng.</p>
                    </div>
                    <div>
                        <p className="text-white font-medium">Tôi có thể nâng cấp gói giữa chừng không?</p>
                        <p className="text-gray-400 text-sm">Có, bạn có thể nâng cấp bất cứ lúc nào và sẽ được tính theo tỷ lệ thời gian còn lại.</p>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}

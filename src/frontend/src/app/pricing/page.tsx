import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

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

export default function PricingPage() {
    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900">
            {/* Navigation */}
            <nav className="border-b border-white/10 backdrop-blur-sm">
                <div className="container mx-auto px-4 py-4 flex items-center justify-between">
                    <Link href="/" className="flex items-center gap-2">
                        <span className="text-2xl">🤖</span>
                        <span className="text-xl font-bold text-white">Trợ Lý KOC</span>
                    </Link>
                    <div className="flex items-center gap-4">
                        <Link href="/login">
                            <Button variant="ghost" className="text-white hover:text-white hover:bg-white/10">
                                Đăng nhập
                            </Button>
                        </Link>
                        <Link href="/register">
                            <Button className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700">
                                Đăng ký
                            </Button>
                        </Link>
                    </div>
                </div>
            </nav>

            {/* Pricing Section */}
            <section className="container mx-auto px-4 py-20">
                <div className="text-center mb-12">
                    <Badge variant="secondary" className="mb-4 bg-purple-500/20 text-purple-300 border-purple-500/30">
                        💎 Bảng Giá
                    </Badge>
                    <h1 className="text-4xl md:text-5xl font-bold text-white mb-4">
                        Chọn Gói Phù Hợp
                    </h1>
                    <p className="text-xl text-gray-300 max-w-2xl mx-auto">
                        Bắt đầu miễn phí, nâng cấp khi cần thiết
                    </p>
                </div>

                <div className="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
                    {tiers.map((tier) => (
                        <Card
                            key={tier.id}
                            className={`relative ${tier.popular
                                    ? "bg-gradient-to-br from-purple-600/20 to-pink-600/20 border-purple-500 scale-105"
                                    : "bg-white/5 border-white/10"
                                }`}
                        >
                            {tier.popular && (
                                <div className="absolute -top-4 left-1/2 -translate-x-1/2">
                                    <Badge className="bg-gradient-to-r from-purple-600 to-pink-600 text-white px-4 py-1">
                                        🔥 Phổ biến nhất
                                    </Badge>
                                </div>
                            )}
                            <CardHeader className="text-center pt-8">
                                <CardTitle className="text-white text-2xl">{tier.name}</CardTitle>
                                <CardDescription className="mt-4">
                                    <span className="text-4xl font-bold text-white">
                                        {tier.price === 0 ? "Miễn phí" : `${tier.price.toLocaleString()}đ`}
                                    </span>
                                    {tier.price > 0 && <span className="text-gray-400">/tháng</span>}
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="space-y-6">
                                <ul className="space-y-3">
                                    {tier.features.map((feature, index) => (
                                        <li key={index} className="flex items-center gap-3 text-gray-300">
                                            <span className="text-green-400">✓</span>
                                            {feature}
                                        </li>
                                    ))}
                                </ul>
                                <Link href="/register">
                                    <Button
                                        className={`w-full ${tier.popular
                                                ? "bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
                                                : "bg-white/10 hover:bg-white/20"
                                            }`}
                                        size="lg"
                                    >
                                        {tier.price === 0 ? "Bắt đầu miễn phí" : "Đăng ký ngay"}
                                    </Button>
                                </Link>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            </section>

            {/* FAQ */}
            <section className="container mx-auto px-4 py-20">
                <h2 className="text-3xl font-bold text-white text-center mb-12">Câu Hỏi Thường Gặp</h2>
                <div className="max-w-2xl mx-auto space-y-6">
                    <Card className="bg-white/5 border-white/10">
                        <CardHeader>
                            <CardTitle className="text-white text-lg">Tôi có thể hủy gói bất cứ lúc nào không?</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-gray-400">
                                Có, bạn có thể hủy gói bất cứ lúc nào. Gói sẽ vẫn hoạt động đến hết chu kỳ thanh toán.
                            </p>
                        </CardContent>
                    </Card>
                    <Card className="bg-white/5 border-white/10">
                        <CardHeader>
                            <CardTitle className="text-white text-lg">Thanh toán bằng những phương thức nào?</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-gray-400">
                                Chúng tôi hỗ trợ thanh toán qua SePay (chuyển khoản ngân hàng), MoMo, và thẻ tín dụng.
                            </p>
                        </CardContent>
                    </Card>
                </div>
            </section>

            {/* Footer */}
            <footer className="border-t border-white/10 py-8">
                <div className="container mx-auto px-4 text-center text-gray-400">
                    <p>© 2026 Trợ Lý KOC. Made with ❤️ in Vietnam</p>
                </div>
            </footer>
        </div>
    );
}

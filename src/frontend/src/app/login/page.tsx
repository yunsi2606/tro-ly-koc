"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { toast } from "sonner";
import { api } from "@/lib/api";

export default function LoginPage() {
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(false);
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);

        try {
            const result = await api.login(email, password);

            if (result.data?.token) {
                api.setToken(result.data.token);
                toast.success("Đăng nhập thành công!");
                router.push("/dashboard");
            } else {
                toast.error(result.error || "Đăng nhập thất bại");
            }
        } catch (error) {
            toast.error("Lỗi kết nối server");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex items-center justify-center p-4">
            <Card className="w-full max-w-md bg-white/5 border-white/10">
                <CardHeader className="text-center">
                    <Link href="/" className="flex items-center justify-center gap-2 mb-4">
                        <span className="text-3xl">🤖</span>
                        <span className="text-2xl font-bold text-white">Trợ Lý KOC</span>
                    </Link>
                    <CardTitle className="text-white text-2xl">Đăng Nhập</CardTitle>
                    <CardDescription className="text-gray-400">
                        Nhập thông tin để truy cập tài khoản
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div className="space-y-2">
                            <label className="text-sm text-gray-300">Email</label>
                            <Input
                                type="email"
                                placeholder="your@email.com"
                                className="bg-white/10 border-white/20 text-white placeholder:text-gray-500"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                required
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm text-gray-300">Mật khẩu</label>
                            <Input
                                type="password"
                                placeholder="••••••••"
                                className="bg-white/10 border-white/20 text-white placeholder:text-gray-500"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                            />
                        </div>
                        <Button
                            type="submit"
                            className="w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
                            disabled={isLoading}
                        >
                            {isLoading ? "Đang đăng nhập..." : "Đăng Nhập"}
                        </Button>
                    </form>

                    <div className="mt-6 text-center">
                        <p className="text-gray-400">
                            Chưa có tài khoản?{" "}
                            <Link href="/register" className="text-purple-400 hover:text-purple-300">
                                Đăng ký ngay
                            </Link>
                        </p>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}

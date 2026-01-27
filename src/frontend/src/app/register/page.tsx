"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { toast } from "sonner";
import { api } from "@/lib/api";

export default function RegisterPage() {
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(false);
    const [formData, setFormData] = useState({
        name: "",
        email: "",
        password: "",
        confirmPassword: "",
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (formData.password !== formData.confirmPassword) {
            toast.error("Mật khẩu xác nhận không khớp");
            return;
        }

        setIsLoading(true);

        try {
            const result = await api.register(formData.name, formData.email, formData.password);

            if (result.data?.token) {
                api.setToken(result.data.token);
                toast.success("Đăng ký thành công!");
                router.push("/dashboard");
            } else {
                toast.error(result.error || "Đăng ký thất bại");
            }
        } catch (error) {
            toast.error("Lỗi kết nối server");
        } finally {
            setIsLoading(false);
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData((prev) => ({
            ...prev,
            [e.target.name]: e.target.value,
        }));
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex items-center justify-center p-4">
            <Card className="w-full max-w-md bg-white/5 border-white/10">
                <CardHeader className="text-center">
                    <Link href="/" className="flex items-center justify-center gap-2 mb-4">
                        <span className="text-3xl">🤖</span>
                        <span className="text-2xl font-bold text-white">Trợ Lý KOC</span>
                    </Link>
                    <CardTitle className="text-white text-2xl">Đăng Ký</CardTitle>
                    <CardDescription className="text-gray-400">
                        Tạo tài khoản và nhận 10 lượt render miễn phí
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div className="space-y-2">
                            <label className="text-sm text-gray-300">Họ tên</label>
                            <Input
                                type="text"
                                name="name"
                                placeholder="Nguyễn Văn A"
                                className="bg-white/10 border-white/20 text-white placeholder:text-gray-500"
                                value={formData.name}
                                onChange={handleChange}
                                required
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm text-gray-300">Email</label>
                            <Input
                                type="email"
                                name="email"
                                placeholder="your@email.com"
                                className="bg-white/10 border-white/20 text-white placeholder:text-gray-500"
                                value={formData.email}
                                onChange={handleChange}
                                required
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm text-gray-300">Mật khẩu</label>
                            <Input
                                type="password"
                                name="password"
                                placeholder="••••••••"
                                className="bg-white/10 border-white/20 text-white placeholder:text-gray-500"
                                value={formData.password}
                                onChange={handleChange}
                                required
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm text-gray-300">Xác nhận mật khẩu</label>
                            <Input
                                type="password"
                                name="confirmPassword"
                                placeholder="••••••••"
                                className="bg-white/10 border-white/20 text-white placeholder:text-gray-500"
                                value={formData.confirmPassword}
                                onChange={handleChange}
                                required
                            />
                        </div>
                        <Button
                            type="submit"
                            className="w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
                            disabled={isLoading}
                        >
                            {isLoading ? "Đang tạo tài khoản..." : "Đăng Ký"}
                        </Button>
                    </form>

                    <div className="mt-6 text-center">
                        <p className="text-gray-400">
                            Đã có tài khoản?{" "}
                            <Link href="/login" className="text-purple-400 hover:text-purple-300">
                                Đăng nhập
                            </Link>
                        </p>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}

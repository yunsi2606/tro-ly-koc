"use client";

import { ReactNode, useEffect, useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import { api, Subscription } from "@/lib/api";

const sidebarLinks = [
    { href: "/dashboard", label: "Dashboard", icon: "📊" },
    { href: "/dashboard/studio", label: "AI Studio", icon: "🎬" },
    { href: "/dashboard/jobs", label: "Lịch sử", icon: "📋" },
    { href: "/dashboard/wallet", label: "Ví tiền", icon: "💳" },
    { href: "/dashboard/subscription", label: "Gói đăng ký", icon: "⭐" },
];

export default function DashboardLayout({ children }: { children: ReactNode }) {
    const pathname = usePathname();
    const router = useRouter();
    const [subscription, setSubscription] = useState<Subscription | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        // Check auth
        const token = api.getToken();
        if (!token) {
            router.push("/login");
            return;
        }

        // Fetch subscription for sidebar
        api.getCurrentSubscription().then((res) => {
            if (res.data) setSubscription(res.data);
            setIsLoading(false);
        });
    }, [router]);

    const handleLogout = () => {
        api.logout();
        router.push("/login");
    };

    return (
        <div className="min-h-screen bg-slate-950 flex">
            {/* Sidebar */}
            <aside className="w-64 bg-slate-900 border-r border-slate-800 flex flex-col">
                <div className="p-4 border-b border-slate-800">
                    <Link href="/" className="flex items-center gap-2">
                        <span className="text-2xl">🤖</span>
                        <span className="text-xl font-bold text-white">Trợ Lý KOC</span>
                    </Link>
                </div>

                <nav className="flex-1 p-4 space-y-2">
                    {sidebarLinks.map((link) => (
                        <Link key={link.href} href={link.href}>
                            <Button
                                variant={pathname === link.href ? "secondary" : "ghost"}
                                className="w-full justify-start gap-3 text-base"
                            >
                                <span>{link.icon}</span>
                                {link.label}
                            </Button>
                        </Link>
                    ))}
                </nav>

                <div className="p-4 border-t border-slate-800">
                    {isLoading ? (
                        <Skeleton className="h-32 bg-slate-800" />
                    ) : (
                        <div className="bg-gradient-to-r from-purple-600/20 to-pink-600/20 rounded-lg p-4 text-center">
                            <p className="text-sm text-gray-400 mb-2">Gói hiện tại</p>
                            <p className="text-lg font-bold text-white">{subscription?.tierName || "Free"}</p>
                            <p className="text-xs text-gray-500 mb-3">
                                {subscription?.remainingJobs ?? 10} lượt còn lại
                            </p>
                            <Link href="/dashboard/subscription">
                                <Button size="sm" className="w-full bg-gradient-to-r from-purple-600 to-pink-600">
                                    Nâng cấp
                                </Button>
                            </Link>
                        </div>
                    )}
                </div>
            </aside>

            {/* Main Content */}
            <div className="flex-1 flex flex-col">
                {/* Top Header */}
                <header className="h-16 bg-slate-900 border-b border-slate-800 flex items-center justify-between px-6">
                    <h1 className="text-xl font-semibold text-white">
                        {sidebarLinks.find((l) => l.href === pathname)?.label || "Dashboard"}
                    </h1>

                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button variant="ghost" className="gap-2">
                                <Avatar className="h-8 w-8">
                                    <AvatarImage src="" />
                                    <AvatarFallback className="bg-purple-600">U</AvatarFallback>
                                </Avatar>
                                <span className="text-white">User</span>
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent className="w-56" align="end">
                            <DropdownMenuLabel>Tài khoản</DropdownMenuLabel>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem>👤 Hồ sơ</DropdownMenuItem>
                            <DropdownMenuItem>⚙️ Cài đặt</DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem className="text-red-500" onClick={handleLogout}>
                                🚪 Đăng xuất
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </header>

                {/* Page Content */}
                <main className="flex-1 p-6 overflow-auto">
                    {children}
                </main>
            </div>
        </div>
    );
}

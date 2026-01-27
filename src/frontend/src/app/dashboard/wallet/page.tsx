"use client";

import { useEffect, useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { api, Wallet, Transaction, PaymentInfo } from "@/lib/api";
import { toast } from "sonner";

export default function WalletPage() {
    const [wallet, setWallet] = useState<Wallet | null>(null);
    const [transactions, setTransactions] = useState<Transaction[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [topUpAmount, setTopUpAmount] = useState("");
    const [isTopUpLoading, setIsTopUpLoading] = useState(false);
    const [paymentInfo, setPaymentInfo] = useState<PaymentInfo | null>(null);
    const [showPaymentModal, setShowPaymentModal] = useState(false);

    const fetchData = async () => {
        setIsLoading(true);
        try {
            const [walletRes, txRes] = await Promise.all([
                api.getWallet(),
                api.getTransactions(),
            ]);

            if (walletRes.data) setWallet(walletRes.data);
            if (txRes.data) setTransactions(txRes.data);
        } catch (error) {
            toast.error("Lỗi tải dữ liệu ví");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, []);

    const handleQuickTopUp = async (amount: number) => {
        setIsTopUpLoading(true);
        try {
            const result = await api.topUp(amount);
            if (result.data) {
                setPaymentInfo(result.data);
                setShowPaymentModal(true);
                toast.success("Vui lòng quét mã QR để thanh toán");
            } else {
                toast.error(result.error || "Không thể tạo yêu cầu thanh toán");
            }
        } catch (error) {
            toast.error("Lỗi tạo yêu cầu nạp tiền");
        } finally {
            setIsTopUpLoading(false);
        }
    };

    const handleCustomTopUp = async () => {
        const amount = parseInt(topUpAmount);
        if (!amount || amount < 10000) {
            toast.error("Số tiền tối thiểu là 10,000 VNĐ");
            return;
        }
        await handleQuickTopUp(amount);
    };

    const handleClosePaymentModal = () => {
        setShowPaymentModal(false);
        setPaymentInfo(null);
        fetchData(); // Refresh wallet data in case payment completed
    };

    if (isLoading) {
        return (
            <div className="space-y-6">
                <Skeleton className="h-8 w-48 bg-slate-800" />
                <div className="grid md:grid-cols-3 gap-6">
                    <Skeleton className="h-40 md:col-span-2 bg-slate-800" />
                    <Skeleton className="h-40 bg-slate-800" />
                </div>
                <Skeleton className="h-64 bg-slate-800" />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-white">Ví Tiền</h1>
                <p className="text-gray-400">Quản lý số dư và lịch sử giao dịch</p>
            </div>

            {/* Balance Card */}
            <div className="grid md:grid-cols-3 gap-6">
                <Card className="bg-gradient-to-br from-purple-600 to-pink-600 border-0 md:col-span-2">
                    <CardContent className="p-6">
                        <p className="text-white/80 mb-2">Số dư hiện tại</p>
                        <p className="text-4xl font-bold text-white mb-4">
                            {wallet?.balance?.toLocaleString() || 0} {wallet?.currency || "VNĐ"}
                        </p>
                        <div className="flex gap-3">
                            <Button
                                className="bg-white text-purple-600 hover:bg-gray-100"
                                onClick={() => handleQuickTopUp(100000)}
                                disabled={isTopUpLoading}
                            >
                                💳 Nạp 100K
                            </Button>
                            <Button variant="outline" className="border-white/30 text-white hover:bg-white/10" onClick={fetchData}>
                                🔄 Làm mới
                            </Button>
                        </div>
                    </CardContent>
                </Card>

                <Card className="bg-slate-900 border-slate-800">
                    <CardHeader>
                        <CardTitle className="text-white text-lg">Nạp Nhanh</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-3">
                        <div className="grid grid-cols-2 gap-2">
                            {[50000, 100000, 200000, 500000].map((amount) => (
                                <Button
                                    key={amount}
                                    variant="outline"
                                    className="text-white"
                                    onClick={() => handleQuickTopUp(amount)}
                                    disabled={isTopUpLoading}
                                >
                                    {(amount / 1000).toFixed(0)}K
                                </Button>
                            ))}
                        </div>
                        <div className="flex gap-2">
                            <Input
                                placeholder="Số tiền khác"
                                className="bg-slate-800 border-slate-700 text-white"
                                type="number"
                                value={topUpAmount}
                                onChange={(e) => setTopUpAmount(e.target.value)}
                            />
                            <Button
                                className="bg-purple-600"
                                onClick={handleCustomTopUp}
                                disabled={isTopUpLoading}
                            >
                                Nạp
                            </Button>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Transaction History */}
            <Card className="bg-slate-900 border-slate-800">
                <CardHeader>
                    <CardTitle className="text-white">Lịch Sử Giao Dịch</CardTitle>
                    <CardDescription>Tất cả giao dịch trong tài khoản ({transactions.length} giao dịch)</CardDescription>
                </CardHeader>
                <CardContent>
                    {transactions.length === 0 ? (
                        <div className="p-8 text-center text-gray-400">
                            <p className="text-4xl mb-4">📭</p>
                            <p>Chưa có giao dịch nào</p>
                        </div>
                    ) : (
                        <div className="space-y-3">
                            {transactions.map((tx) => (
                                <div key={tx.id} className="flex items-center justify-between p-4 bg-slate-800 rounded-lg">
                                    <div className="flex items-center gap-4">
                                        <div className={`w-10 h-10 rounded-full flex items-center justify-center ${tx.type === "TOPUP" ? "bg-green-500/20" :
                                            tx.type === "REFUND" ? "bg-blue-500/20" : "bg-red-500/20"
                                            }`}>
                                            {tx.type === "TOPUP" ? "💰" : tx.type === "REFUND" ? "↩️" : "💸"}
                                        </div>
                                        <div>
                                            <p className="text-white font-medium">{tx.description}</p>
                                            <p className="text-sm text-gray-400">
                                                {new Date(tx.createdAt).toLocaleString("vi-VN")}
                                            </p>
                                        </div>
                                    </div>
                                    <div className="text-right">
                                        <p className={`font-bold ${tx.amount >= 0 ? "text-green-400" : "text-red-400"}`}>
                                            {tx.amount >= 0 ? "+" : ""}{tx.amount.toLocaleString()} VNĐ
                                        </p>
                                        <Badge variant="outline" className="text-xs">
                                            {tx.type}
                                        </Badge>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </CardContent>
            </Card>

            {/* Payment QR Modal */}
            <Dialog open={showPaymentModal} onOpenChange={handleClosePaymentModal}>
                <DialogContent className="bg-slate-900 border-slate-800 max-w-md">
                    <DialogHeader>
                        <DialogTitle className="text-white text-center">Nạp tiền vào ví</DialogTitle>
                        <DialogDescription className="text-center">
                            Quét mã QR hoặc chuyển khoản theo thông tin bên dưới
                        </DialogDescription>
                    </DialogHeader>

                    {paymentInfo && (
                        <div className="space-y-4">
                            {/* QR Code */}
                            <div className="flex justify-center">
                                <img
                                    src={paymentInfo.qrCodeUrl}
                                    alt="QR Thanh toán"
                                    className="w-64 h-64 rounded-lg border border-slate-700"
                                />
                            </div>

                            {/* Bank Info */}
                            <div className="bg-slate-800 rounded-lg p-4 space-y-3">
                                <div className="flex justify-between">
                                    <span className="text-gray-400">Ngân hàng</span>
                                    <span className="text-white font-medium">{paymentInfo.bankName}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-400">Số tài khoản</span>
                                    <span className="text-white font-medium font-mono">{paymentInfo.bankAccount}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-400">Chủ tài khoản</span>
                                    <span className="text-white font-medium">{paymentInfo.accountName}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-400">Số tiền</span>
                                    <span className="text-green-400 font-bold">{paymentInfo.amount.toLocaleString()} VNĐ</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-400">Nội dung CK</span>
                                    <span className="text-yellow-400 font-mono text-sm">{paymentInfo.content}</span>
                                </div>
                            </div>

                            <p className="text-center text-xs text-gray-500">
                                Hệ thống sẽ tự động cập nhật số dư sau khi nhận được thanh toán (1-5 phút)
                            </p>

                            <Button
                                className="w-full bg-purple-600 hover:bg-purple-700"
                                onClick={handleClosePaymentModal}
                            >
                                Đã thanh toán xong
                            </Button>
                        </div>
                    )}
                </DialogContent>
            </Dialog>
        </div>
    );
}

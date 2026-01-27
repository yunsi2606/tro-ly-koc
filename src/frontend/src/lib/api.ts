/**
 * API Client for Trợ Lý KOC Backend
 */

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api";

interface ApiResponse<T> {
    data?: T;
    error?: string;
    status: number;
}

class ApiClient {
    private token: string | null = null;

    setToken(token: string) {
        this.token = token;
        if (typeof window !== "undefined") {
            localStorage.setItem("auth_token", token);
        }
    }

    getToken(): string | null {
        if (this.token) return this.token;
        if (typeof window !== "undefined") {
            return localStorage.getItem("auth_token");
        }
        return null;
    }

    clearToken() {
        this.token = null;
        if (typeof window !== "undefined") {
            localStorage.removeItem("auth_token");
        }
    }

    private async request<T>(
        endpoint: string,
        options: RequestInit = {}
    ): Promise<ApiResponse<T>> {
        const token = this.getToken();
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` }),
            ...options.headers,
        };

        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, {
                ...options,
                headers,
            });

            const data = response.ok ? await response.json() : null;

            return {
                data,
                status: response.status,
                error: response.ok ? undefined : `Error ${response.status}`,
            };
        } catch (error) {
            return {
                status: 0,
                error: error instanceof Error ? error.message : "Network error",
            };
        }
    }

    // File Upload
    async uploadFile(file: File, folder: string = "inputs"): Promise<ApiResponse<{ url: string }>> {
        const token = this.getToken();
        const formData = new FormData();
        formData.append("file", file);
        formData.append("folder", folder);

        try {
            const response = await fetch(`${API_BASE_URL}/files/upload`, {
                method: "POST",
                headers: {
                    ...(token && { Authorization: `Bearer ${token}` }),
                },
                body: formData,
            });

            const data = response.ok ? await response.json() : null;
            return {
                data,
                status: response.status,
                error: response.ok ? undefined : `Upload failed: ${response.status}`,
            };
        } catch (error) {
            return {
                status: 0,
                error: error instanceof Error ? error.message : "Upload error",
            };
        }
    }

    // Auth
    async login(email: string, password: string) {
        return this.request<{ token: string; user: User }>("/auth/login", {
            method: "POST",
            body: JSON.stringify({ email, password }),
        });
    }

    async register(name: string, email: string, password: string) {
        return this.request<{ token: string; user: User }>("/auth/register", {
            method: "POST",
            body: JSON.stringify({ name, email, password }),
        });
    }

    async logout() {
        this.clearToken();
    }

    // Jobs
    async getJobs() {
        return this.request<Job[]>("/jobs/my-jobs");
    }

    async getJob(id: string) {
        return this.request<Job>(`/jobs/${id}`);
    }

    async createJob(jobData: CreateJobRequest) {
        return this.request<Job>("/jobs", {
            method: "POST",
            body: JSON.stringify(jobData),
        });
    }

    // Wallet
    async getWallet() {
        return this.request<Wallet>("/wallet");
    }

    async getTransactions() {
        return this.request<Transaction[]>("/wallet/transactions");
    }

    async topUp(amount: number) {
        return this.request<{ paymentUrl: string }>("/wallet/topup", {
            method: "POST",
            body: JSON.stringify({ amount }),
        });
    }

    // Subscription
    async getCurrentSubscription() {
        return this.request<Subscription>("/subscription/current");
    }

    async getSubscriptionTiers() {
        return this.request<SubscriptionTier[]>("/subscription/tiers");
    }

    async subscribeTier(tierId: string) {
        return this.request<Subscription>(`/subscription/subscribe/${tierId}`, {
            method: "POST",
        });
    }

    async cancelSubscription() {
        return this.request<void>("/subscription/cancel", {
            method: "POST",
        });
    }
}

// Types
export interface User {
    id: string;
    email: string;
    name: string;
    createdAt: string;
}

export interface Job {
    id: string;
    userId: string;
    jobType: "TalkingHead" | "VirtualTryOn" | "ImageToVideo" | "MotionTransfer" | "FaceSwap";
    status: "Pending" | "Queued" | "Processing" | "Completed" | "Failed";
    sourceImageUrl?: string;
    audioUrl?: string;
    outputUrl?: string;
    processingTimeMs?: number;
    errorMessage?: string;
    createdAt: string;
    completedAt?: string;
}

export interface CreateJobRequest {
    jobType: string;
    sourceImageUrl?: string;
    audioUrl?: string;
    garmentImageUrl?: string;
    skeletonVideoUrl?: string;
    targetFaceUrl?: string;
    outputResolution?: string;
    priority?: string;
}

export interface Wallet {
    id: string;
    userId: string;
    balance: number;
    currency: string;
}

export interface Transaction {
    id: string;
    walletId: string;
    type: "TOPUP" | "DEDUCT" | "REFUND";
    amount: number;
    description: string;
    createdAt: string;
}

export interface Subscription {
    id: string;
    userId: string;
    tierId: string;
    tierName: string;
    status: "Active" | "Expired" | "Cancelled";
    remainingJobs: number;
    periodEnd: string;
}

export interface SubscriptionTier {
    id: string;
    name: string;
    price: number;
    maxJobsPerMonth: number;
    maxResolution: string;
    hasWatermark: boolean;
    features: string[];
}

export const api = new ApiClient();

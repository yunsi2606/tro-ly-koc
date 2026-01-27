/**
 * SignalR Hub connection for real-time job updates
 */

// Note: Install @microsoft/signalr: npm install @microsoft/signalr

import * as signalR from "@microsoft/signalr";

const HUB_URL = process.env.NEXT_PUBLIC_SIGNALR_URL || "http://localhost:5000/hubs/jobs";

class JobsHubConnection {
    private connection: signalR.HubConnection | null = null;
    private listeners: Map<string, (job: JobUpdate) => void> = new Map();

    async connect(token: string) {
        if (this.connection) {
            await this.disconnect();
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, {
                accessTokenFactory: () => token,
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Handle job updates
        this.connection.on("JobStatusChanged", (update: JobUpdate) => {
            console.log("Job update received:", update);
            this.notifyListeners(update);
        });

        this.connection.on("JobCompleted", (update: JobUpdate) => {
            console.log("Job completed:", update);
            this.notifyListeners(update);
        });

        this.connection.on("JobFailed", (update: JobUpdate) => {
            console.log("Job failed:", update);
            this.notifyListeners(update);
        });

        // Handle connection events
        this.connection.onreconnecting(() => {
            console.log("SignalR reconnecting...");
        });

        this.connection.onreconnected(() => {
            console.log("SignalR reconnected");
        });

        this.connection.onclose(() => {
            console.log("SignalR connection closed");
        });

        try {
            await this.connection.start();
            console.log("SignalR connected");
        } catch (error) {
            console.error("SignalR connection failed:", error);
            throw error;
        }
    }

    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }

    // Subscribe to job updates
    onJobUpdate(jobId: string, callback: (job: JobUpdate) => void): () => void {
        this.listeners.set(jobId, callback);
        return () => { this.listeners.delete(jobId); };
    }

    // Subscribe to all updates
    onAnyJobUpdate(callback: (job: JobUpdate) => void): () => void {
        this.listeners.set("*", callback);
        return () => { this.listeners.delete("*"); };
    }
    private notifyListeners(update: JobUpdate) {
        // Notify specific job listener
        const specificListener = this.listeners.get(update.jobId);
        if (specificListener) {
            specificListener(update);
        }

        // Notify catch-all listeners
        const catchAllListener = this.listeners.get("*");
        if (catchAllListener) {
            catchAllListener(update);
        }
    }

    // Check connection state
    get isConnected(): boolean {
        return this.connection?.state === signalR.HubConnectionState.Connected;
    }
}

export interface JobUpdate {
    jobId: string;
    status: string;
    progress?: number;
    outputUrl?: string;
    error?: string;
}

export const jobsHub = new JobsHubConnection();

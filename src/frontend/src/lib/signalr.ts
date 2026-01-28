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
        // Normalize properties (Backend sends PascalCase, Frontend expects camelCase)
        const jobId = update.jobId || update.JobId || "";
        const status = update.status || update.Status || "Unknown";
        const outputUrl = update.outputUrl || update.OutputUrl;
        const error = update.error || update.Error;

        // Merge normalized values back for the listener
        const normalizedUpdate: JobUpdate = {
            ...update,
            jobId,
            status,
            outputUrl,
            error
        };

        // Notify specific job listener
        if (jobId) {
            const specificListener = this.listeners.get(jobId);
            if (specificListener) {
                specificListener(normalizedUpdate);
            }
        }

        // Notify catch-all listeners
        const catchAllListener = this.listeners.get("*");
        if (catchAllListener) {
            catchAllListener(normalizedUpdate);
        }
    }

    // Check connection state
    get isConnected(): boolean {
        return this.connection?.state === signalR.HubConnectionState.Connected;
    }
}

export interface JobUpdate {
    jobId: string;
    JobId?: string; // Handle PascalCase from backend

    status: string;
    Status?: string; // Handle PascalCase

    outputUrl?: string;
    OutputUrl?: string; // Handle PascalCase

    error?: string;
    Error?: string; // Handle PascalCase

    progress?: number;
    completedAt?: string;
    CompletedAt?: string;
}

export const jobsHub = new JobsHubConnection();

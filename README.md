# Trợ Lý KOC - AI Video Generator Platform

![TroLiKOC Banner](https://via.placeholder.com/1200x400?text=Tro+Ly+KOC+Platform)

**Trợ Lý KOC** is a SaaS platform enabling KOCs/Reviewers to automatically generate high-quality AI videos. The system integrates advanced AI technologies (LivePortrait, IDM-VTON, SVD-XT, MimicMotion, FaceFusion) to provide features like Talking Head, Virtual Try-on, and Face Swap.

## 🚀 Key Features

*   **Talking Head**: Animate portrait photos with lip-sync from audio.
*   **Virtual Try-On**: Virtual clothing try-on, compositing garments onto models.
*   **Image to Video**: Convert static images into short videos.
*   **Motion Transfer**: Transfer body motion from a reference video to a character image.
*   **Face Swap**: Swap faces in existing videos.

---

## 🛠️ Technology Stack

### Backend (.NET 10)
*   **Architecture**: Modular Monolith
*   **Framework**: ASP.NET Core 10
*   **Communication**: MassTransit (RabbitMQ), SignalR (Real-time updates)
*   **Database**: SQL Server (EF Core), Redis (Caching)
*   **Scheduling**: Quartz.NET

### Frontend (Next.js 14)
*   **Framework**: Next.js (App Router), React, TypeScript
*   **Styling**: Tailwind CSS, Shadcn/UI
*   **State Management**: Zustand, React Query

### AI Worker (Python 3.11)
*   **Framework**: PyTorch, CUDA
*   **Features**: GPU-accelerated inference, Queue-based processing
*   **Libraries**: Diffusers, InsightFace, FFMPEG

### Infrastructure
*   **Containerization**: Docker, Docker Compose
*   **Storage**: MinIO (S3 Compatible)
*   **Gateway**: Cloudflare Tunnel

---

## ⚙️ Installation Guide (Local Development)

### Prerequisites
*   **Docker Desktop** & **Git**
*   **NVIDIA GPU** (VRAM >= 8GB) & **NVIDIA Container Toolkit** (for AI Worker)
*   **.NET 10 SDK** (Optional, if running backend outside docker)
*   **Node.js 20+** (Optional, if running frontend outside docker)

### Step 1: Clone Repository
```bash
git clone https://github.com/your-repo/tro-li-koc.git
cd tro-li-koc
```

### Step 2: Environment Configuration
Copy the `.env.example` file to `.env` in the `docker/` directory:
```bash
# In docker/ folder
cp .env.example .env
```
*Note: Update environment variables such as `CLOUDFLARE_TUNNEL_TOKEN` if necessary.*

### Step 3: Start the System

Run the entire system using Docker Compose:

```bash
cd docker
docker-compose up -d --build
```

Services will be available at:
*   **Frontend**: http://localhost:3000
*   **Backend API**: http://localhost:5500 (Swagger: http://localhost:5500/swagger)
*   **RabbitMQ**: http://localhost:15679 (User: `admin`, Pass: `admin123`)
*   **MinIO**: http://localhost:9031 (User: `minioadmin`, Pass: `minioadmin123`)

---

## 📂 Project Structure

```
tro-li-koc/
├── docker/                 # Docker configuration (Compose, Env)
├── docs/                   # Documentation & ADRs
├── scripts/                # Helper scripts
├── src/
│   ├── ai-worker/          # Python AI Worker service
│   ├── backend/            # .NET 10 Modular Monolith API
│   │   ├── src/
│   │   │   ├── TroLiKOC.API/           # Main Entry Point
│   │   │   ├── TroLiKOC.SharedKernel/  # Shared code
│   │   │   └── Modules/                # Domain Modules (Identity, Jobs, etc.)
│   └── frontend/           # Next.js Web App
└── README.md
```

## 📚 Detailed Documentation (ADRs)

Key architectural decisions are recorded in [docs/adr](docs/adr/):

*   [ADR 001: Database Choice (SQL Server + Redis)](docs/adr/001-database-choice.md)
*   [ADR 002: MassTransit Version](docs/adr/002-masstransit-version.md)
*   [ADR 004: AI Worker Architecture](docs/adr/004-ai-worker-architecture.md)
*   [ADR 005: RabbitMQ RabbitMQ Routing](docs/adr/005-rabbitmq-masstransit-routing.md)
*   [ADR 006: Quartz.NET Configuration](docs/adr/006-quartznet-jobstore-configuration.md)

---

## 📝 License
This project is proprietary software. All rights reserved.

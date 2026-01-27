# 🤖 Trợ Lý KOC - AI Worker

Python service that processes AI rendering jobs from RabbitMQ using GPU-accelerated models.

## 🎯 Supported Job Types & Models

| Job Type | Primary Model | Fallback | HuggingFace Model |
|----------|---------------|----------|-------------------|
| **TalkingHead** | LivePortrait | SadTalker | Custom installation |
| **VirtualTryOn** | IDM-VTON | SD Inpainting | `yisol/IDM-VTON` |
| **ImageToVideo** | SVD-XT | - | `stabilityai/stable-video-diffusion-img2vid-xt` |
| **MotionTransfer** | MimicMotion | AnimateDiff | `guoyww/animatediff-motion-adapter-v1-5-2` |
| **FaceSwap** | InsightFace | - | `deepinsight/inswapper` |

## 📁 Project Structure

```
src/ai-worker/
├── main.py                 # Entry point
├── requirements.txt        # Python dependencies
├── Dockerfile             # GPU-enabled Docker image
├── .env.example           # Environment config template
├── scripts/
│   └── download_models.py # Model download utility
└── worker/
    ├── config.py          # Settings management
    ├── message_consumer.py # RabbitMQ consumer
    ├── job_dispatcher.py  # Routes jobs to processors
    ├── storage.py         # MinIO integration
    └── processors/
        ├── base.py            # Abstract base processor
        ├── talking_head.py    # LivePortrait/SadTalker
        ├── virtual_tryon.py   # IDM-VTON/SD Inpainting
        ├── image_to_video.py  # SVD-XT (Diffusers)
        ├── motion_transfer.py # MimicMotion/AnimateDiff
        └── face_swap.py       # InsightFace + inswapper
```

## 🚀 Quick Start

### 1. Install Dependencies

```bash
# Create virtual environment
python -m venv venv
source venv/bin/activate  # Linux/Mac
# or: venv\Scripts\activate  # Windows

# Install PyTorch with CUDA
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121

# Install other dependencies
pip install -r requirements.txt
```

### 2. Download Models

```bash
# Download all models (~30GB)
python scripts/download_models.py --all

# Or download specific models
python scripts/download_models.py --svd      # SVD-XT (Image to Video)
python scripts/download_models.py --face     # InsightFace + inswapper
python scripts/download_models.py --pose     # ControlNet pose
python scripts/download_models.py --motion   # AnimateDiff
python scripts/download_models.py --inpaint  # SD Inpainting
```

### 3. Configure Environment

```bash
cp .env.example .env
# Edit .env with your settings
```

### 4. Run Worker

```bash
python main.py
```

## 🐳 Docker Deployment

### Build Image

```bash
docker build -t trolikoc-ai-worker .
```

### Run with GPU

```bash
# Requires NVIDIA Container Toolkit
docker run --gpus all \
    -v /path/to/models:/models \
    -e RABBITMQ_HOST=rabbitmq \
    -e MINIO_ENDPOINT=minio:9000 \
    --network trolikoc_trolikoc-network \
    trolikoc-ai-worker
```

### Docker Compose (with GPU)

```yaml
ai-worker:
  build:
    context: ../src/ai-worker
    dockerfile: Dockerfile
  container_name: trolikoc-ai-worker
  volumes:
    - ai_models:/models  # Persistent model cache
  environment:
    RABBITMQ_HOST: rabbitmq
    MINIO_ENDPOINT: minio:9000
    DEVICE: cuda
  deploy:
    resources:
      reservations:
        devices:
          - driver: nvidia
            count: 1
            capabilities: [gpu]
```

## 🖥️ GPU Requirements

| Model | Min VRAM | Recommended | Notes |
|-------|----------|-------------|-------|
| SVD-XT | 8GB | 16GB | Uses model CPU offloading |
| InsightFace | 2GB | 4GB | ONNX runtime |
| AnimateDiff | 6GB | 12GB | SD 1.5 based |
| SD Inpainting | 4GB | 8GB | SD 1.5 based |
| **All Models** | 8GB | **24GB (A5000/4090)** | With offloading |

## ⚙️ Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `RABBITMQ_HOST` | RabbitMQ server | `localhost` |
| `RABBITMQ_USER` | RabbitMQ username | `admin` |
| `RABBITMQ_PASS` | RabbitMQ password | `admin123` |
| `MINIO_ENDPOINT` | MinIO server | `localhost:9000` |
| `MINIO_ACCESS_KEY` | MinIO access key | `minioadmin` |
| `MINIO_SECRET_KEY` | MinIO secret key | `minioadmin123` |
| `DEVICE` | PyTorch device | `cuda` |
| `WORKER_CONCURRENCY` | Concurrent jobs | `1` |
| `MODEL_CACHE_DIR` | Model cache path | `~/.trolikoc_models` |

## 📝 Message Format

### Job Request (from .NET API)

```json
{
    "jobId": "uuid",
    "userId": "uuid",
    "sourceImageUrl": "https://minio:9000/inputs/...",
    "audioUrl": "https://minio:9000/inputs/...",
    "priority": "high",
    "outputResolution": "720p",
    "numFrames": 25,
    "fps": 6
}
```

### Job Completion (to .NET API)

```json
{
    "jobId": "uuid",
    "status": "COMPLETED",
    "outputUrl": "http://minio:9000/trolikoc-outputs/...",
    "processingTimeMs": 45000,
    "error": null
}
```

## 🔧 Processor Details

### ImageToVideo (SVD-XT)
- **Input**: Single image
- **Output**: 25 frames @ 6fps (~4 seconds)
- **Params**: `motionBucketId` (1-255), `noiseAugStrength` (0-1)

### TalkingHead (LivePortrait)
- **Input**: Portrait image + Audio file
- **Output**: Lip-synced video
- **Params**: `expressionScale`, `enhanceFace`

### VirtualTryOn (IDM-VTON)
- **Input**: Person image + Garment image
- **Output**: Person wearing garment
- **Params**: `garmentCategory` (upper_body/lower_body/dresses)

### MotionTransfer (MimicMotion)
- **Input**: Person image + Driving video
- **Output**: Person with transferred motion
- **Params**: `numFrames`, `fps`

### FaceSwap (InsightFace)
- **Input**: Source video + Target face image
- **Output**: Video with swapped face
- **Params**: `swapAllFaces`, `enhanceFace`

## 🌐 Cloud Deployment

### RunPod Serverless

1. Build Docker image and push to registry
2. Create RunPod template with GPU worker
3. Configure environment variables
4. Deploy and scale as needed

### AWS/GCP GPU Instances

- AWS: `g4dn.xlarge` (T4) or `p3.2xlarge` (V100)
- GCP: `n1-standard-8` with T4 GPU
- Use persistent SSD for model cache

## 🔄 Development Mode

When running without GPU or models, processors fall back to FFmpeg-based placeholders:
- **TalkingHead**: Creates video from image + audio overlay
- **VirtualTryOn**: Returns model image
- **ImageToVideo**: Creates looping video from image
- **MotionTransfer**: Returns driving video
- **FaceSwap**: Returns source video

## 📊 Monitoring

Check worker logs:
```bash
docker logs -f trolikoc-ai-worker
```

RabbitMQ Management: http://localhost:15672
- Monitor queue depths
- Check consumer connections
- View message rates

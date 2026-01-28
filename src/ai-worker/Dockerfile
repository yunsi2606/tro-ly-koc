# Trợ Lý KOC - AI Worker Dockerfile
# GPU-enabled container for AI processing

FROM nvidia/cuda:12.1.1-cudnn8-runtime-ubuntu22.04

# Prevent interactive prompts
ENV DEBIAN_FRONTEND=noninteractive

# Set working directory
WORKDIR /app

# System Dependencies
RUN apt-get update && apt-get install -y \
    python3.11 \
    python3.11-venv \
    python3.11-dev \
    python3-pip \
    ffmpeg \
    libgl1-mesa-glx \
    libglib2.0-0 \
    libsm6 \
    libxext6 \
    libxrender-dev \
    libgomp1 \
    git \
    wget \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Python Environment
RUN python3.11 -m venv /opt/venv
ENV PATH="/opt/venv/bin:$PATH"

# Upgrade pip
RUN pip install --upgrade pip wheel setuptools

# PyTorch with CUDA 12.1
RUN pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121

# Python Dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Model Cache Directory
ENV HF_HOME=/models/huggingface
ENV TRANSFORMERS_CACHE=/models/huggingface
ENV TORCH_HOME=/models/torch
ENV INSIGHTFACE_MODEL_DIR=/models/insightface

RUN mkdir -p /models/huggingface /models/torch /models/insightface

# Application Code
COPY . .
# Environment Variables
ENV PYTHONUNBUFFERED=1
ENV PYTHONDONTWRITEBYTECODE=1
ENV DEVICE=cuda

# Pre-download Models (Optional)
# Uncomment to bake models into image
# RUN python scripts/download_models.py --all

# ===========================================
# Install LivePortrait
# ===========================================
RUN git clone https://github.com/KwaiVGI/LivePortrait /app/LivePortrait
# Add src to pythonpath so "import config" works if needed, or root for "import src"
ENV PYTHONPATH="/app/LivePortrait:/app/LivePortrait/src:$PYTHONPATH"

# Install LivePortrait specific requirements if not covered
RUN pip install -r /app/LivePortrait/requirements.txt

# ===========================================
# Install SadTalker
# ===========================================
RUN git clone https://github.com/OpenTalker/SadTalker /app/SadTalker
ENV SADTALKER_PATH="/app/SadTalker"
# SadTalker requirements usually overlap, but running it ensures connection
# Note: SadTalker requirements might be old, so we rely on our main requirements first
# RUN pip install -r /app/SadTalker/requirements.txt

# Health Check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD python -c "import torch; print(f'CUDA: {torch.cuda.is_available()}')" || exit 1

# Entrypoint
CMD ["python", "main.py"]

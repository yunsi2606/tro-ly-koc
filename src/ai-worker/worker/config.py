"""
Configuration settings for the AI Worker.
Uses pydantic-settings for environment variable management.
"""

import os
from pydantic_settings import BaseSettings
from typing import Optional


class Settings(BaseSettings):
    """Application settings loaded from environment variables."""
    
    # RabbitMQ
    rabbitmq_host: str = "localhost"
    rabbitmq_port: int = 5672
    rabbitmq_user: str = "admin"
    rabbitmq_pass: str = "admin123"
    rabbitmq_vhost: str = "/"
    
    # MinIO
    minio_endpoint: str = "localhost:9000"
    minio_access_key: str = "minioadmin"
    minio_secret_key: str = "minioadmin123"
    minio_secure: bool = False
    minio_bucket: str = "trolikoc-outputs"
    
    # API Callback
    api_base_url: str = "http://localhost:5000"
    
    # Worker Settings
    worker_concurrency: int = 1  # Number of concurrent jobs (limited by GPU memory)
    prefetch_count: int = 1      # Messages to prefetch from RabbitMQ
    
    # Model Paths (optional, can use default HuggingFace cache)
    model_cache_dir: Optional[str] = None
    
    # GPU Settings
    device: str = "cuda"  # cuda or cpu
    
    class Config:
        env_prefix = ""
        case_sensitive = False
        env_file = ".env"


# Queue names for each job type
QUEUE_NAMES = {
    "TalkingHead": "talking-head-queue",
    "VirtualTryOn": "virtual-tryon-queue",
    "ImageToVideo": "img2video-queue",
    "MotionTransfer": "motion-transfer-queue",
    "FaceSwap": "face-swap-queue",
}

# Routing keys for topic exchange
ROUTING_KEYS = {
    "TalkingHead": "job.talking-head",
    "VirtualTryOn": "job.virtual-tryon",
    "ImageToVideo": "job.img2video",
    "MotionTransfer": "job.motion-transfer",
    "FaceSwap": "job.face-swap",
}

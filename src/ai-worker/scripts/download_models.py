#!/usr/bin/env python3
"""
Model Download Script
Downloads and caches all required AI models for Trợ Lý KOC AI Worker.

Usage:
    python scripts/download_models.py [--all|--svd|--face|--pose]
"""

import os
import sys
import argparse
import logging
from pathlib import Path

logging.basicConfig(level=logging.INFO, format='%(asctime)s | %(message)s')
logger = logging.getLogger(__name__)

# Default model cache directory
MODEL_DIR = os.environ.get("MODEL_CACHE_DIR", Path.home() / ".trolikoc_models")


def download_svd_xt():
    """Download Stable Video Diffusion XT model."""
    logger.info("📥 Downloading SVD-XT (Stable Video Diffusion)...")
    
    from diffusers import StableVideoDiffusionPipeline
    import torch
    
    pipe = StableVideoDiffusionPipeline.from_pretrained(
        "stabilityai/stable-video-diffusion-img2vid-xt",
        torch_dtype=torch.float16,
        variant="fp16"
    )
    
    logger.info("✅ SVD-XT downloaded and cached")
    del pipe


def download_insightface():
    """Download InsightFace models for face analysis."""
    logger.info("📥 Downloading InsightFace models...")
    
    from insightface.app import FaceAnalysis
    
    app = FaceAnalysis(name='buffalo_l')
    app.prepare(ctx_id=-1)  # CPU mode for download
    
    logger.info("✅ InsightFace buffalo_l downloaded")


def download_inswapper():
    """Download inswapper model for face swapping."""
    logger.info("📥 Downloading inswapper model...")
    
    import urllib.request
    
    model_dir = Path.home() / ".insightface" / "models"
    model_dir.mkdir(parents=True, exist_ok=True)
    
    model_path = model_dir / "inswapper_128.onnx"
    
    if model_path.exists():
        logger.info("✅ inswapper_128.onnx already exists")
        return
    
    # Download from HuggingFace
    url = "https://huggingface.co/deepinsight/inswapper/resolve/main/inswapper_128.onnx"
    
    logger.info(f"   Downloading from {url}...")
    urllib.request.urlretrieve(url, model_path)
    
    logger.info(f"✅ inswapper_128.onnx saved to {model_path}")


def download_controlnet_pose():
    """Download ControlNet pose detection models."""
    logger.info("📥 Downloading ControlNet pose models...")
    
    from controlnet_aux import DWposeDetector
    
    detector = DWposeDetector()
    
    logger.info("✅ DWpose detector downloaded")


def download_animatediff():
    """Download AnimateDiff motion adapter."""
    logger.info("📥 Downloading AnimateDiff motion adapter...")
    
    from diffusers import MotionAdapter
    
    adapter = MotionAdapter.from_pretrained("guoyww/animatediff-motion-adapter-v1-5-2")
    
    logger.info("✅ AnimateDiff motion adapter downloaded")
    del adapter


def download_sd_inpainting():
    """Download SD Inpainting for VirtualTryOn fallback."""
    logger.info("📥 Downloading SD Inpainting...")
    
    from diffusers import StableDiffusionInpaintPipeline
    import torch
    
    pipe = StableDiffusionInpaintPipeline.from_pretrained(
        "runwayml/stable-diffusion-inpainting",
        torch_dtype=torch.float16
    )
    
    logger.info("✅ SD Inpainting downloaded")
    del pipe


def main():
    parser = argparse.ArgumentParser(description="Download AI models for Trợ Lý KOC")
    parser.add_argument("--all", action="store_true", help="Download all models")
    parser.add_argument("--svd", action="store_true", help="Download SVD-XT (Image to Video)")
    parser.add_argument("--face", action="store_true", help="Download face models (InsightFace, inswapper)")
    parser.add_argument("--pose", action="store_true", help="Download pose models (ControlNet)")
    parser.add_argument("--motion", action="store_true", help="Download motion models (AnimateDiff)")
    parser.add_argument("--inpaint", action="store_true", help="Download SD Inpainting (Try-On)")
    
    args = parser.parse_args()
    
    # Default to all if no specific model selected
    if not any([args.all, args.svd, args.face, args.pose, args.motion, args.inpaint]):
        args.all = True
    
    logger.info("=" * 60)
    logger.info("🚀 Trợ Lý KOC - Model Downloader")
    logger.info("=" * 60)
    
    try:
        if args.all or args.svd:
            download_svd_xt()
        
        if args.all or args.face:
            download_insightface()
            download_inswapper()
        
        if args.all or args.pose:
            download_controlnet_pose()
        
        if args.all or args.motion:
            download_animatediff()
        
        if args.all or args.inpaint:
            download_sd_inpainting()
        
        logger.info("=" * 60)
        logger.info("🎉 All models downloaded successfully!")
        logger.info("=" * 60)
        
    except Exception as e:
        logger.error(f"❌ Error: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()

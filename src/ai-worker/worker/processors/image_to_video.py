"""
Image to Video Processor
Uses Stable Video Diffusion XT (SVD-XT) to animate still images.

Model: SVD-XT (https://huggingface.co/stabilityai/stable-video-diffusion-img2vid-xt)
Input: Source image
Output: Animated video (25 frames, ~4 seconds at 6fps)

Requirements:
- torch with CUDA
- diffusers >= 0.25.0
- transformers
- accelerate
"""

import logging
import os
from typing import Dict, Any, Optional

import torch
from PIL import Image

from worker.processors.base import BaseProcessor
from worker.config import Settings
from worker.storage import StorageService

logger = logging.getLogger(__name__)


class ImageToVideoProcessor(BaseProcessor):
    """Processor for Image-to-Video (SVD-XT) jobs."""
    
    def __init__(self, settings: Settings, storage: StorageService):
        super().__init__(settings, storage)
        self.model_name = "SVD-XT"
        self._pipeline = None
    
    def load_model(self):
        """Load Stable Video Diffusion XT model."""
        try:
            from diffusers import StableVideoDiffusionPipeline
            from diffusers.utils import export_to_video
            
            logger.info(f"🔄 Đang tải {self.model_name} từ HuggingFace...")
            
            # Load pipeline with fp16 for memory efficiency
            self._pipeline = StableVideoDiffusionPipeline.from_pretrained(
                "stabilityai/stable-video-diffusion-img2vid-xt",
                torch_dtype=torch.float16,
                variant="fp16"
            )
            
            # Move to GPU
            if self.device == "cuda" and torch.cuda.is_available():
                self._pipeline.to("cuda")
                # Enable memory optimizations
                self._pipeline.enable_model_cpu_offload()
                # Use xformers if available
                try:
                    self._pipeline.enable_xformers_memory_efficient_attention()
                    logger.info("✅ XFormers memory efficient attention enabled")
                except Exception:
                    logger.info("ℹ️ XFormers not available, using default attention")
            else:
                self._pipeline.to("cpu")
                logger.warning("⚠️ Running on CPU - this will be slow!")
            
            self._model = "loaded"
            logger.info(f"✅ {self.model_name} đã sẵn sàng")
            
        except ImportError as e:
            logger.error(f"❌ Không thể tải diffusers: {e}")
            logger.info("📦 Chạy: pip install diffusers transformers accelerate")
            self._model = "placeholder"
        except Exception as e:
            logger.error(f"❌ Lỗi tải model: {e}")
            self._model = "placeholder"
    
    async def process(self, payload: Dict[str, Any]) -> str:
        """
        Process an Image-to-Video job.
        
        Payload fields:
        - sourceImageUrl: URL to the source image
        - outputResolution: Target resolution (576p, 720p, 1080p)
        - numFrames: Number of frames (default: 25)
        - fps: Frames per second (default: 6)
        - motionBucketId: Motion intensity (1-255, default: 127)
        - noiseAugStrength: Noise augmentation (0-1, default: 0.02)
        """
        self.ensure_model_loaded()
        
        job_id = payload.get("jobId") or payload.get("JobId")
        source_image_url = payload.get("sourceImageUrl") or payload.get("SourceImageUrl")
        resolution = payload.get("outputResolution") or payload.get("OutputResolution") or "576p"
        num_frames = int(payload.get("numFrames") or payload.get("NumFrames") or 25)
        fps = int(payload.get("fps") or payload.get("Fps") or 6)
        motion_bucket_id = int(payload.get("motionBucketId") or payload.get("MotionBucketId") or 127)
        noise_aug_strength = float(payload.get("noiseAugStrength") or payload.get("NoiseAugStrength") or 0.02)
        
        logger.info(f"🎥 Xử lý ImageToVideo: {job_id}")
        logger.info(f"   - Ảnh nguồn: {source_image_url}")
        logger.info(f"   - Độ phân giải: {resolution}")
        logger.info(f"   - Số frame: {num_frames}")
        logger.info(f"   - FPS: {fps}")
        logger.info(f"   - Motion bucket: {motion_bucket_id}")
        
        # Download inputs
        inputs = await self.download_inputs({
            "source": source_image_url
        })
        
        # Output path
        output_path = os.path.join(self.temp_dir, f"{job_id}_video.mp4")
        
        # Process
        if self._model == "placeholder":
            # Development: create simple video from image
            await self._create_placeholder_video(inputs["source"], output_path, fps)
        else:
            # Production: use SVD-XT
            await self._generate_video(
                inputs["source"],
                output_path,
                resolution=resolution,
                num_frames=num_frames,
                fps=fps,
                motion_bucket_id=motion_bucket_id,
                noise_aug_strength=noise_aug_strength
            )
        
        logger.info(f"✅ ImageToVideo hoàn thành: {output_path}")
        return output_path
    
    async def _generate_video(
        self,
        image_path: str,
        output_path: str,
        resolution: str = "576p",
        num_frames: int = 25,
        fps: int = 6,
        motion_bucket_id: int = 127,
        noise_aug_strength: float = 0.02
    ):
        """Generate video using SVD-XT pipeline."""
        from diffusers.utils import export_to_video
        
        # Load and resize image
        image = Image.open(image_path).convert("RGB")
        
        # Get target size based on resolution
        res_map = {
            "576p": (1024, 576),
            "720p": (1280, 720),
            "1080p": (1920, 1080)
        }
        target_size = res_map.get(resolution, (1024, 576))
        
        # Resize image maintaining aspect ratio
        image = self._resize_image(image, target_size)
        
        logger.info(f"🎬 Generating {num_frames} frames at {target_size}...")
        
        # Set random seed for reproducibility
        generator = torch.manual_seed(42)
        
        # Generate frames
        with torch.inference_mode():
            frames = self._pipeline(
                image,
                num_frames=num_frames,
                decode_chunk_size=8,  # Memory optimization
                motion_bucket_id=motion_bucket_id,
                noise_aug_strength=noise_aug_strength,
                generator=generator
            ).frames[0]
        
        logger.info(f"✅ Generated {len(frames)} frames")
        
        # Export to video
        export_to_video(frames, output_path, fps=fps)
        
        logger.info(f"📹 Video saved to: {output_path}")
    
    def _resize_image(self, image: Image.Image, target_size: tuple) -> Image.Image:
        """Resize image to target size maintaining aspect ratio."""
        target_w, target_h = target_size
        
        # Calculate aspect ratios
        img_ratio = image.width / image.height
        target_ratio = target_w / target_h
        
        if img_ratio > target_ratio:
            # Image is wider - fit to width
            new_w = target_w
            new_h = int(target_w / img_ratio)
        else:
            # Image is taller - fit to height
            new_h = target_h
            new_w = int(target_h * img_ratio)
        
        # Resize
        image = image.resize((new_w, new_h), Image.Resampling.LANCZOS)
        
        # Create canvas and paste
        canvas = Image.new("RGB", target_size, (0, 0, 0))
        paste_x = (target_w - new_w) // 2
        paste_y = (target_h - new_h) // 2
        canvas.paste(image, (paste_x, paste_y))
        
        return canvas
    
    async def _create_placeholder_video(self, image_path: str, output_path: str, fps: int = 6):
        """Create a placeholder video from image using FFmpeg."""
        import subprocess
        
        duration = 4  # seconds
        
        cmd = [
            "ffmpeg", "-y",
            "-loop", "1",
            "-i", image_path,
            "-c:v", "libx264",
            "-t", str(duration),
            "-pix_fmt", "yuv420p",
            "-vf", "scale=1024:576:force_original_aspect_ratio=decrease,pad=1024:576:(ow-iw)/2:(oh-ih)/2",
            "-r", str(fps),
            output_path
        ]
        
        try:
            subprocess.run(cmd, check=True, capture_output=True)
        except subprocess.CalledProcessError as e:
            logger.error(f"FFmpeg error: {e.stderr.decode()}")
            raise

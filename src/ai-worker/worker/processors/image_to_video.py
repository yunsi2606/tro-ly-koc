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
            # Using SVD-XT 1.1 (25 frames) for better quality and movement
            model_id = "stabilityai/stable-video-diffusion-img2vid-xt-1-1"
            
            logger.info(f"🔄 Đang tải {self.model_name} ({model_id})...")
            
            self._pipeline = StableVideoDiffusionPipeline.from_pretrained(
                model_id,
                torch_dtype=torch.float16,
                variant="fp16",
                low_cpu_mem_usage=True  # Critical for 6GB RAM env
            )
            
            # Move to GPU
            if self.device == "cuda" and torch.cuda.is_available():
                logger.info(f"🚀 GPU detected: {torch.cuda.get_device_name(0)}")
                vram = torch.cuda.get_device_properties(0).total_memory / 1024**3
                logger.info(f"   VRAM: {vram:.2f} GB")

                # self._pipeline.to("cuda")  <-- REMOVE THIS. enable_model_cpu_offload handles device placement.
                
                # Enable memory optimizations
                # enable_model_cpu_offload() runs model on GPU but offloads parts to CPU when not used
                self._pipeline.enable_model_cpu_offload()
                logger.info("✅ Model CPU offload enabled")
                
                # Further memory optimizations for low VRAM
                try:
                    self._pipeline.enable_vae_slicing()
                    self._pipeline.enable_vae_tiling()
                    logger.info("✅ VAE slicing & tiling enabled")
                except Exception as e:
                    logger.warning(f"⚠️ Could not enable VAE slicing/tiling: {e}")
                
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
    
    
    def unload_model(self):
        """Unload SVD pipeline."""
        self._pipeline = None
        super().unload_model()
    
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
        num_inference_steps = int(payload.get("numInferenceSteps") or payload.get("NumInferenceSteps") or 15)
        
        logger.info(f"🎥 Xử lý ImageToVideo: {job_id}")
        logger.info(f"   - Ảnh nguồn: {source_image_url}")
        logger.info(f"   - Độ phân giải: {resolution}")
        logger.info(f"   - Số frame: {num_frames}")
        logger.info(f"   - FPS: {fps}")
        logger.info(f"   - Steps: {num_inference_steps}")
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
            # Production: use SVD
            await self._generate_video(
                inputs["source"],
                output_path,
                resolution=resolution,
                num_frames=num_frames,
                fps=fps,
                num_inference_steps=num_inference_steps,
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
        num_inference_steps: int = 15,
        motion_bucket_id: int = 127,
        noise_aug_strength: float = 0.02
    ):
        """Generate video using SVD-XT pipeline."""
        from diffusers.utils import export_to_video
        
        # Load and resize image
        image = Image.open(image_path).convert("RGB")
        target_size = (1024, 576)
        if resolution == "720p": target_size = (1280, 720)
        elif resolution == "1080p": target_size = (1920, 1080)
        
        # Resize image maintaining aspect ratio
        image = self._resize_image(image, target_size)
        
        logger.info(f"🎬 Generating {num_frames} frames at {target_size} using SVD-XT ({num_inference_steps} steps)...")
        
        # Set random seed
        generator = torch.manual_seed(42)
        
        def progress_callback(pipe, step: int, timestep: int, callback_kwargs: Dict[str, Any]):
            logger.info(f"⏳ Generating: Step {step}/{num_inference_steps} (Timestep {timestep})")
            return callback_kwargs

        # Generate frames
        with torch.inference_mode():
            frames = self._pipeline(
                image,
                num_frames=num_frames,
                decode_chunk_size=2,  # Lower for stability on low VRAM with XT
                num_inference_steps=num_inference_steps,
                motion_bucket_id=motion_bucket_id,
                noise_aug_strength=noise_aug_strength,
                generator=generator,
                callback_on_step_end=progress_callback
            ).frames[0]
        
        logger.info(f"✅ Generated {len(frames)} frames")
        
        # Intermediate raw export
        raw_path = output_path.replace(".mp4", "_raw.mp4")
        export_to_video(frames, raw_path, fps=fps)
        
        # Interpolate to smooth 24fps
        await self._interpolate_video(raw_path, output_path)
    
    async def _interpolate_video(self, input_path: str, output_path: str):
        """Use FFmpeg minterpolate to smooth video to 24fps."""
        import subprocess
        import asyncio
        logger.info("🌊 Interpolating video to 24fps for smoothness...")
        
        # motion interpolation (optimized for speed/quality balance)
        # mi_mode=mci: Motion Compensated Interpolation
        # mc_mode=obmc: Overlapped Block Motion Compensation (Faster than aobmc)
        # me_mode=bilat: Bilateral motion estimation (Faster than bidir)
        
        cmd = [
            "ffmpeg", "-y",
            "-i", input_path,
            "-filter:v", "minterpolate='mi_mode=mci:mc_mode=obmc:me_mode=bilat:fps=24'",
            "-c:v", "libx264",
            "-pix_fmt", "yuv420p",
            "-preset", "veryfast",  # Faster encoding
            output_path
        ]
        
        # Execute in thread to avoid blocking event loop too much (though subprocess.run blocks)
        try:
             # Run ffmpeg
            process = await asyncio.create_subprocess_exec(
                *cmd,
                stdout=asyncio.subprocess.PIPE,
                stderr=asyncio.subprocess.PIPE
            )
            stdout, stderr = await process.communicate()
            
            if process.returncode != 0:
                logger.error(f"FFmpeg interpolation failed: {stderr.decode()}")
                # Fallback to copy raw
                import shutil
                shutil.copy(input_path, output_path)
            else:
                logger.info("✅ Interpolation complete")
                
        except Exception as e:
            logger.error(f"Interpolation error: {e}")
            import shutil
            shutil.copy(input_path, output_path)

    def _resize_image(self, image: Image.Image, target_size: tuple) -> Image.Image:
        """Resize image to target size maintaining aspect ratio."""
        target_w, target_h = target_size
        img_ratio = image.width / image.height
        target_ratio = target_w / target_h
        
        if img_ratio > target_ratio:
            new_w = target_w
            new_h = int(target_w / img_ratio)
        else:
            new_h = target_h
            new_w = int(target_h * img_ratio)
        
        image = image.resize((new_w, new_h), Image.Resampling.LANCZOS)
        canvas = Image.new("RGB", target_size, (0, 0, 0))
        canvas.paste(image, ((target_w - new_w) // 2, (target_h - new_h) // 2))
        return canvas
    
    async def _create_placeholder_video(self, image_path: str, output_path: str, fps: int = 6):
        """Create a placeholder video from image using FFmpeg."""
        import subprocess
        # ... (keep existing placeholder logic or simplified)
        cmd = ["ffmpeg", "-y", "-loop", "1", "-i", image_path, "-t", "4", "-vf", "scale=1024:576", "-r", "24", output_path]
        subprocess.run(cmd, check=False)


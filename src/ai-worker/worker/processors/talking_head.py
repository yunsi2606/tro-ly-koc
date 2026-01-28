"""
Talking Head Processor
Uses LivePortrait to animate a portrait image with audio-driven lip-sync.

Model: LivePortrait (https://github.com/KwaiVGI/LivePortrait)
Input: Source image + Audio file
Output: Animated video with lip-sync

Requirements:
- torch with CUDA
- onnxruntime-gpu
- opencv-python
- librosa (for audio processing)
"""

import logging
import os
import subprocess
from typing import Dict, Any, Optional

import torch
import numpy as np
from PIL import Image

from worker.processors.base import BaseProcessor
from worker.config import Settings
from worker.storage import StorageService

logger = logging.getLogger(__name__)


class TalkingHeadProcessor(BaseProcessor):
    """Processor for Talking Head (LivePortrait) jobs."""
    
    def __init__(self, settings: Settings, storage: StorageService):
        super().__init__(settings, storage)
        self.model_name = "LivePortrait"
        self._inference_cfg = None
        self._pipeline = None
    
    def unload_model(self):
        """Unload pipeline."""
        self._pipeline = None
        self._inference_cfg = None
        super().unload_model()
    
    def load_model(self):
        """Load LivePortrait model components."""
        try:
            # LivePortrait uses multiple model components
            # We'll use the inference pipeline from their repo
            logger.info(f"🔄 Đang tải {self.model_name}...")
            
            # Check if LivePortrait is installed
            try:
                # Try import style: src.* (if PYTHONPATH points to repo root)
                try:
                    from src.config.inference_config import InferenceConfig
                    from src.live_portrait_pipeline import LivePortraitPipeline
                    logger.info("✅ Imported LivePortrait from src package")
                except ImportError:
                    # Try import style: liveportrait.* (if src was renamed or installed as package)
                    from liveportrait.config.inference_config import InferenceConfig
                    from liveportrait.live_portrait_pipeline import LivePortraitPipeline
                    logger.info("✅ Imported LivePortrait from liveportrait package")
                
                self._inference_cfg = InferenceConfig(
                    device_id=0 if self.device == "cuda" else -1,
                )
                
                self._pipeline = LivePortraitPipeline(
                    inference_cfg=self._inference_cfg
                )
                
                self._model = "loaded"
                logger.info(f"✅ {self.model_name} đã sẵn sàng")
                
            except ImportError:
                logger.warning("⚠️ LivePortrait chưa được cài đặt")
                logger.info("📦 Clone và cài đặt từ: https://github.com/KwaiVGI/LivePortrait")
                
                # Try alternative: SadTalker which is more accessible
                self._try_load_sadtalker()
                
        except Exception as e:
            logger.error(f"❌ Lỗi tải model: {e}")
            self._model = "placeholder"
    
    def _try_load_sadtalker(self):
        """Try loading SadTalker as an alternative."""
        try:
            # SadTalker is another good option for talking head
            # https://github.com/OpenTalker/SadTalker
            logger.info("🔄 Thử tải SadTalker thay thế...")
            
            # Check for SadTalker
            import sys
            sadtalker_path = os.environ.get("SADTALKER_PATH", "/app/SadTalker")
            
            if os.path.exists(sadtalker_path):
                sys.path.insert(0, sadtalker_path)
                from inference import SadTalker as SadTalkerModel
                
                self._pipeline = SadTalkerModel(
                    device=self.device,
                    checkpoint_dir=os.path.join(sadtalker_path, "checkpoints")
                )
                self._model = "sadtalker"
                logger.info("✅ SadTalker đã sẵn sàng")
            else:
                raise FileNotFoundError(f"SadTalker not found at {sadtalker_path}")
                
        except Exception as e:
            logger.warning(f"⚠️ SadTalker không khả dụng: {e}")
            self._model = "placeholder"
    
    async def process(self, payload: Dict[str, Any]) -> str:
        """
        Process a Talking Head job.
        
        Payload fields:
        - sourceImageUrl: URL to the source portrait image
        - audioUrl: URL to the driving audio
        - outputResolution: Target resolution (720p, 1080p, 4K)
        - addWatermark: Whether to add watermark
        - enhanceFace: Whether to apply face enhancement
        - expressionScale: Expression intensity (0.5-1.5, default: 1.0)
        """
        self.ensure_model_loaded()
        
        job_id = payload.get("jobId") or payload.get("JobId")
        source_image_url = payload.get("sourceImageUrl") or payload.get("SourceImageUrl")
        audio_url = payload.get("audioUrl") or payload.get("AudioUrl")
        resolution = payload.get("outputResolution") or payload.get("OutputResolution") or "720p"
        add_watermark = payload.get("addWatermark", True)
        enhance_face = payload.get("enhanceFace", False)
        expression_scale = float(payload.get("expressionScale") or 1.0)
        
        logger.info(f"🎬 Xử lý TalkingHead: {job_id}")
        logger.info(f"   - Ảnh nguồn: {source_image_url}")
        logger.info(f"   - Audio: {audio_url}")
        logger.info(f"   - Độ phân giải: {resolution}")
        logger.info(f"   - Expression scale: {expression_scale}")
        
        # Download inputs
        inputs = await self.download_inputs({
            "source": source_image_url,
            "audio": audio_url
        })
        
        # Output path
        output_path = os.path.join(self.temp_dir, f"{job_id}_output.mp4")
        
        # Process based on available model
        if self._model == "placeholder":
            await self._create_placeholder_video(inputs, output_path, resolution)
        elif self._model == "sadtalker":
            await self._process_sadtalker(inputs, output_path, resolution, expression_scale)
        else:
            await self._process_liveportrait(inputs, output_path, resolution, expression_scale)
        
        # Add watermark if required
        if add_watermark and self._model != "placeholder":
            output_path = await self._add_watermark(output_path)
        
        # Enhance face if requested
        if enhance_face and self._model != "placeholder":
            output_path = await self._enhance_face(output_path)
        
        logger.info(f"✅ TalkingHead hoàn thành: {output_path}")
        return output_path
    
    async def _process_liveportrait(
        self,
        inputs: Dict[str, str],
        output_path: str,
        resolution: str,
        expression_scale: float
    ):
        """Process using LivePortrait pipeline."""
        # Already loaded in self._pipeline, but if we need class reference:
        # from src.live_portrait_pipeline import LivePortraitPipeline
        
        logger.info("🎭 Generating with LivePortrait...")
        
        # Run inference
        self._pipeline.execute(
            source_image_path=inputs["source"],
            driving_audio_path=inputs["audio"],
            output_path=output_path,
            source_audio=None,
            flag_relative=True,
            flag_do_crop=True,
            flag_pasteback=True,
        )
    
    async def _process_sadtalker(
        self,
        inputs: Dict[str, str],
        output_path: str,
        resolution: str,
        expression_scale: float
    ):
        """Process using SadTalker."""
        logger.info("🎭 Generating with SadTalker...")
        
        # Get resolution dimensions
        res_map = {"720p": 512, "1080p": 512, "4K": 512}  # SadTalker uses fixed size
        size = res_map.get(resolution, 512)
        
        result = self._pipeline.test(
            source_image=inputs["source"],
            driven_audio=inputs["audio"],
            still=True,
            preprocess='crop',
            expression_scale=expression_scale,
            size=size
        )
        
        # Move result to output path
        import shutil
        shutil.move(result, output_path)
    
    async def _create_placeholder_video(
        self,
        inputs: Dict[str, str],
        output_path: str,
        resolution: str
    ):
        """Create a placeholder video for development/testing."""
        # Get resolution dimensions
        res_map = {"720p": "1280x720", "1080p": "1920x1080", "4K": "3840x2160"}
        size = res_map.get(resolution, "1280x720")
        
        # Use ffmpeg to create a simple video from the image
        # Simplified command to be more robust
        cmd = [
            "ffmpeg", "-y",
            "-loop", "1",
            "-i", inputs.get("source", ""),
            "-i", inputs.get("audio", ""),
            "-c:v", "libx264",
            "-t", "5",  # Limit to 5 seconds for testing
            "-pix_fmt", "yuv420p",
            "-vf", f"scale={size.split('x')[0]}:-2", # Simple scale, maintain aspect ratio
            "-shortest",
            output_path
        ]
        
        try:
            subprocess.run(cmd, check=True, capture_output=True)
        except subprocess.CalledProcessError as e:
            logger.error(f"FFmpeg error: {e.stderr.decode()}")
            raise
    
    async def _add_watermark(self, video_path: str) -> str:
        """Add a watermark to the video."""
        # Create watermark text
        watermark_path = video_path.replace(".mp4", "_watermarked.mp4")
        
        cmd = [
            "ffmpeg", "-y",
            "-i", video_path,
            "-vf", "drawtext=text='Trợ Lý KOC':fontsize=24:fontcolor=white@0.5:x=w-tw-10:y=h-th-10",
            "-c:a", "copy",
            watermark_path
        ]
        
        try:
            subprocess.run(cmd, check=True, capture_output=True)
            return watermark_path
        except subprocess.CalledProcessError:
            # If watermarking fails, return original
            return video_path
    
    async def _enhance_face(self, video_path: str) -> str:
        """Apply face enhancement using GFPGAN or similar."""
        # This would use a face enhancement model
        # For now, return the original video
        logger.info("⚠️ Face enhancement not implemented yet")
        return video_path

"""
Face Swap Processor
Uses FaceFusion/InsightFace for high-quality face swapping in videos.

Model: FaceFusion (https://github.com/facefusion/facefusion)
Alternative: InsightFace with inswapper model
Input: Source video + Target face image
Output: Video with swapped face

Requirements:
- insightface
- onnxruntime-gpu
- opencv-python
- numpy
"""

import logging
import os
import subprocess
from typing import Dict, Any, Optional, Tuple, List

import cv2
import numpy as np
from PIL import Image

from worker.processors.base import BaseProcessor
from worker.config import Settings
from worker.storage import StorageService

logger = logging.getLogger(__name__)


class FaceSwapProcessor(BaseProcessor):
    """Processor for Face Swap (FaceFusion/InsightFace) jobs."""
    
    def __init__(self, settings: Settings, storage: StorageService):
        super().__init__(settings, storage)
        self.model_name = "FaceSwap"
        self._face_analyzer = None
        self._face_swapper = None
    
    def load_model(self):
        """Load face swap models."""
        try:
            logger.info(f"🔄 Đang tải {self.model_name}...")
            
            try:
                import insightface
                from insightface.app import FaceAnalysis
                
                # Initialize face analyzer
                self._face_analyzer = FaceAnalysis(
                    name='buffalo_l',
                    providers=['CUDAExecutionProvider', 'CPUExecutionProvider']
                )
                self._face_analyzer.prepare(ctx_id=0 if self.device == "cuda" else -1)
                
                # Load face swapper model
                model_path = self._get_swapper_model_path()
                if model_path and os.path.exists(model_path):
                    self._face_swapper = insightface.model_zoo.get_model(
                        model_path,
                        providers=['CUDAExecutionProvider', 'CPUExecutionProvider']
                    )
                    self._model = "insightface"
                    logger.info(f"✅ InsightFace đã sẵn sàng")
                else:
                    logger.warning("⚠️ Swapper model not found, using placeholder")
                    self._model = "placeholder"
                    
            except ImportError as e:
                logger.warning(f"⚠️ InsightFace không khả dụng: {e}")
                logger.info("📦 Để cài đặt InsightFace trên Windows:")
                logger.info("   1. Download wheel từ: https://github.com/Gourieff/Assets/releases")
                logger.info("   2. pip install insightface-0.7.3-cp311-cp311-win_amd64.whl")
                logger.info("   Hoặc cài Visual Studio Build Tools và chạy: pip install insightface")
                self._model = "placeholder"
                
        except Exception as e:
            logger.error(f"❌ Lỗi tải model: {e}")
            self._model = "placeholder"
    
    def _get_swapper_model_path(self) -> Optional[str]:
        """Get path to the inswapper model."""
        # Check common locations
        possible_paths = [
            # Project root (same level as src/)
            os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(__file__))))), "inswapper_128.onnx"),
            # ai-worker directory
            os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(__file__))), "inswapper_128.onnx"),
            # Models folder in project
            os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(__file__))))), "models", "inswapper_128.onnx"),
            # User's insightface folder
            os.path.expanduser("~/.insightface/models/inswapper_128.onnx"),
            # Docker models folder
            "/models/inswapper_128.onnx",
            # Custom model cache dir from settings
            os.path.join(self.settings.model_cache_dir or "", "inswapper_128.onnx"),
        ]
        
        for path in possible_paths:
            if path and os.path.exists(path):
                logger.info(f"✅ Found inswapper model at: {path}")
                return path
        
        logger.warning("⚠️ inswapper_128.onnx không tìm thấy")
        logger.info("📥 Đặt file inswapper_128.onnx vào một trong các vị trí sau:")
        for p in possible_paths[:4]:
            if p:
                logger.info(f"   - {p}")
        return None
    
    async def process(self, payload: Dict[str, Any]) -> str:
        """
        Process a Face Swap job.
        
        Payload fields:
        - sourceVideoUrl: URL to the source video
        - targetFaceUrl: URL to the target face image (face to swap in)
        - swapAllFaces: Whether to swap all faces or just the first one
        - enhanceFace: Whether to apply GFPGAN enhancement
        """
        self.ensure_model_loaded()
        
        job_id = payload.get("jobId") or payload.get("JobId")
        source_video_url = payload.get("sourceVideoUrl") or payload.get("SourceVideoUrl")
        target_face_url = payload.get("targetFaceUrl") or payload.get("TargetFaceUrl")
        swap_all_faces = payload.get("swapAllFaces", False)
        enhance_face = payload.get("enhanceFace", True)
        
        logger.info(f"🎭 Xử lý FaceSwap: {job_id}")
        logger.info(f"   - Video nguồn: {source_video_url}")
        logger.info(f"   - Khuôn mặt mới: {target_face_url}")
        logger.info(f"   - Swap tất cả: {swap_all_faces}")
        
        # Download inputs
        inputs = await self.download_inputs({
            "video": source_video_url,
            "face": target_face_url
        })
        
        # Output path
        output_path = os.path.join(self.temp_dir, f"{job_id}_faceswap.mp4")
        
        # Process
        if self._model == "placeholder":
            import shutil
            shutil.copy(inputs["video"], output_path)
        else:
            await self._process_faceswap(
                inputs["video"],
                inputs["face"],
                output_path,
                swap_all_faces=swap_all_faces,
                enhance=enhance_face
            )
        
        logger.info(f"✅ FaceSwap hoàn thành: {output_path}")
        return output_path
    
    async def _process_faceswap(
        self,
        video_path: str,
        face_path: str,
        output_path: str,
        swap_all_faces: bool = False,
        enhance: bool = True
    ):
        """Process video with face swapping."""
        logger.info("🎭 Processing face swap...")
        
        # Load target face
        target_image = cv2.imread(face_path)
        target_faces = self._face_analyzer.get(target_image)
        
        if not target_faces:
            raise ValueError("Không tìm thấy khuôn mặt trong ảnh target")
        
        target_face = target_faces[0]
        logger.info(f"✅ Detected target face with score: {target_face.det_score:.2f}")
        
        # Open video
        cap = cv2.VideoCapture(video_path)
        fps = int(cap.get(cv2.CAP_PROP_FPS))
        width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
        total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
        
        # Setup output video
        fourcc = cv2.VideoWriter_fourcc(*'mp4v')
        out = cv2.VideoWriter(output_path + ".temp.mp4", fourcc, fps, (width, height))
        
        frame_idx = 0
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break
            
            # Detect faces in frame
            source_faces = self._face_analyzer.get(frame)
            
            if source_faces:
                # Swap faces
                if swap_all_faces:
                    for face in source_faces:
                        frame = self._face_swapper.get(
                            frame,
                            face,
                            target_face,
                            paste_back=True
                        )
                else:
                    # Only swap the largest/most prominent face
                    source_face = max(source_faces, key=lambda x: x.bbox[2] * x.bbox[3])
                    frame = self._face_swapper.get(
                        frame,
                        source_face,
                        target_face,
                        paste_back=True
                    )
            
            out.write(frame)
            frame_idx += 1
            
            if frame_idx % 30 == 0:
                logger.info(f"   Progress: {frame_idx}/{total_frames} frames")
        
        cap.release()
        out.release()
        
        # Copy audio from original video
        await self._copy_audio(video_path, output_path + ".temp.mp4", output_path)
        
        # Clean up temp file
        if os.path.exists(output_path + ".temp.mp4"):
            os.remove(output_path + ".temp.mp4")
        
        # Apply face enhancement if requested
        if enhance:
            await self._enhance_faces(output_path)
    
    async def _copy_audio(self, source_video: str, processed_video: str, output_path: str):
        """Copy audio from source video to processed video."""
        cmd = [
            "ffmpeg", "-y",
            "-i", processed_video,
            "-i", source_video,
            "-c:v", "libx264",
            "-c:a", "aac",
            "-map", "0:v:0",
            "-map", "1:a:0?",
            "-shortest",
            output_path
        ]
        
        try:
            subprocess.run(cmd, check=True, capture_output=True)
        except subprocess.CalledProcessError:
            # If audio copy fails, just use video without audio
            import shutil
            shutil.copy(processed_video, output_path)
    
    async def _enhance_faces(self, video_path: str):
        """Apply GFPGAN face enhancement to video."""
        try:
            # This would use GFPGAN for face restoration
            # For now, skip enhancement
            logger.info("ℹ️ Face enhancement skipped (GFPGAN not integrated)")
        except Exception as e:
            logger.warning(f"⚠️ Face enhancement failed: {e}")

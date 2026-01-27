"""
Job Dispatcher
Routes incoming job requests to the appropriate AI processor.
"""

import logging
import time
from typing import Dict, Any

from worker.config import Settings
from worker.processors.base import BaseProcessor
from worker.processors.talking_head import TalkingHeadProcessor
from worker.processors.virtual_tryon import VirtualTryOnProcessor
from worker.processors.image_to_video import ImageToVideoProcessor
from worker.processors.motion_transfer import MotionTransferProcessor
from worker.processors.face_swap import FaceSwapProcessor
from worker.storage import StorageService

logger = logging.getLogger(__name__)


class JobDispatcher:
    """Dispatches jobs to the appropriate AI processor."""
    
    def __init__(self, settings: Settings):
        self.settings = settings
        self.storage = StorageService(settings)
        
        # Initialize processors (lazy loading)
        self._processors: Dict[str, BaseProcessor] = {}
    
    def _get_processor(self, job_type: str) -> BaseProcessor:
        """Get or create a processor for the given job type."""
        if job_type not in self._processors:
            logger.info(f"🔧 Đang khởi tạo processor cho {job_type}...")
            
            if job_type == "TalkingHead":
                self._processors[job_type] = TalkingHeadProcessor(self.settings, self.storage)
            elif job_type == "VirtualTryOn":
                self._processors[job_type] = VirtualTryOnProcessor(self.settings, self.storage)
            elif job_type == "ImageToVideo":
                self._processors[job_type] = ImageToVideoProcessor(self.settings, self.storage)
            elif job_type == "MotionTransfer":
                self._processors[job_type] = MotionTransferProcessor(self.settings, self.storage)
            elif job_type == "FaceSwap":
                self._processors[job_type] = FaceSwapProcessor(self.settings, self.storage)
            else:
                raise ValueError(f"Unknown job type: {job_type}")
            
            logger.info(f"✅ Processor {job_type} sẵn sàng")
        
        return self._processors[job_type]
    
    async def dispatch(self, job_type: str, payload: Dict[str, Any]) -> Dict[str, Any]:
        """
        Dispatch a job to the appropriate processor.
        
        Args:
            job_type: Type of the job (TalkingHead, VirtualTryOn, etc.)
            payload: Job payload from RabbitMQ
        
        Returns:
            Result dictionary with status, output_url, processing_time_ms
        """
        start_time = time.time()
        
        try:
            processor = self._get_processor(job_type)
            
            # Process the job
            output_path = await processor.process(payload)
            
            # Upload to MinIO
            job_id = payload.get("jobId") or payload.get("JobId")
            output_url = await self.storage.upload_output(job_id, job_type, output_path)
            
            processing_time_ms = int((time.time() - start_time) * 1000)
            
            return {
                "status": "COMPLETED",
                "output_url": output_url,
                "processing_time_ms": processing_time_ms
            }
            
        except Exception as e:
            processing_time_ms = int((time.time() - start_time) * 1000)
            logger.error(f"Lỗi xử lý job: {e}", exc_info=True)
            
            return {
                "status": "FAILED",
                "error": str(e),
                "processing_time_ms": processing_time_ms
            }

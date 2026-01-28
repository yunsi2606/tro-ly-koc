"""
Base Processor
Abstract base class for all AI processors.
"""

import logging
import os
import tempfile
from abc import ABC, abstractmethod
from typing import Dict, Any

from worker.config import Settings
from worker.storage import StorageService

logger = logging.getLogger(__name__)


class BaseProcessor(ABC):
    """Abstract base class for AI processors."""
    
    def __init__(self, settings: Settings, storage: StorageService):
        self.settings = settings
        self.storage = storage
        self.device = settings.device
        self.temp_dir = tempfile.mkdtemp(prefix="trolikoc_")
        self._model = None
    
    @abstractmethod
    async def process(self, payload: Dict[str, Any]) -> str:
        """
        Process a job and return the path to the output file.
        
        Args:
            payload: Job payload containing input URLs and parameters
        
        Returns:
            Path to the local output file
        """
        pass
    
    @abstractmethod
    def load_model(self):
        """Load the AI model into memory."""
        pass
    
    def ensure_model_loaded(self):
        """Ensure the model is loaded before processing."""
        if self._model is None:
            logger.info(f"🔄 Đang tải model {self.__class__.__name__}...")
            self.load_model()
            logger.info(f"✅ Model {self.__class__.__name__} đã sẵn sàng")
    
    async def download_inputs(self, urls: Dict[str, str]) -> Dict[str, str]:
        """
        Download all input files from URLs.
        
        Args:
            urls: Dictionary mapping input names to URLs
        
        Returns:
            Dictionary mapping input names to local file paths
        """
        local_paths = {}
        for name, url in urls.items():
            if url:
                ext = os.path.splitext(url)[1] or ".tmp"
                local_path = os.path.join(self.temp_dir, f"{name}{ext}")
                await self.storage.download_input(url, local_path)
                local_paths[name] = local_path
                logger.info(f"📥 Đã tải: {name}")
        return local_paths
    
    
    def unload_model(self):
        """Unload the model to free up memory."""
        import gc
        import torch
        
        if self._model is not None:
            logger.info(f"🗑️ Đang giải phóng model {self.__class__.__name__}...")
            self._model = None
            
            # Subclasses should override this to clear their specific pipeline variables
            # e.g. self._pipeline = None
            
            # Force garbage collection
            gc.collect()
            
            # Clear CUDA cache if using GPU
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
                torch.cuda.ipc_collect()
            
            logger.info(f"✅ Đã giải phóng bộ nhớ {self.__class__.__name__}")

    def cleanup(self):
        """Clean up temporary files."""
        import shutil
        if os.path.exists(self.temp_dir):
            shutil.rmtree(self.temp_dir)

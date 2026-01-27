"""
Storage Service
Handles file uploads to MinIO object storage.
"""

import logging
import os
import uuid
from datetime import datetime
from minio import Minio
from minio.error import S3Error

from worker.config import Settings

logger = logging.getLogger(__name__)


class StorageService:
    """Handles file storage operations with MinIO."""
    
    def __init__(self, settings: Settings):
        self.settings = settings
        self.client = Minio(
            settings.minio_endpoint,
            access_key=settings.minio_access_key,
            secret_key=settings.minio_secret_key,
            secure=settings.minio_secure
        )
        self.bucket = settings.minio_bucket
        
        # Ensure bucket exists
        self._ensure_bucket_exists()
    
    def _ensure_bucket_exists(self):
        """Create the bucket if it doesn't exist."""
        try:
            if not self.client.bucket_exists(self.bucket):
                self.client.make_bucket(self.bucket)
                logger.info(f"📦 Đã tạo bucket: {self.bucket}")
        except S3Error as e:
            logger.error(f"Lỗi tạo bucket: {e}")
    
    async def download_input(self, url: str, local_path: str) -> str:
        """
        Download input file from URL to local path.
        Supports both MinIO URLs and external URLs.
        """
        import aiohttp
        import aiofiles
        
        # Create directory if needed
        os.makedirs(os.path.dirname(local_path), exist_ok=True)
        
        # If it's a MinIO URL, extract bucket and object name
        if self.settings.minio_endpoint in url:
            # Parse MinIO URL
            # Format: http://minio:9000/bucket/object-key
            parts = url.split("/")
            bucket = parts[-2]
            object_name = parts[-1]
            
            self.client.fget_object(bucket, object_name, local_path)
        else:
            # Download from external URL
            async with aiohttp.ClientSession() as session:
                async with session.get(url) as response:
                    if response.status == 200:
                        async with aiofiles.open(local_path, 'wb') as f:
                            await f.write(await response.read())
                    else:
                        raise Exception(f"Failed to download: {url}, status: {response.status}")
        
        return local_path
    
    async def upload_output(self, job_id: str, job_type: str, local_path: str) -> str:
        """
        Upload output file to MinIO and return the URL.
        
        Args:
            job_id: Job ID for organizing outputs
            job_type: Type of job for folder structure
            local_path: Local path to the output file
        
        Returns:
            Public URL to the uploaded file
        """
        # Generate object name
        ext = os.path.splitext(local_path)[1]
        date_path = datetime.now().strftime("%Y/%m/%d")
        object_name = f"outputs/{job_type.lower()}/{date_path}/{job_id}{ext}"
        
        # Determine content type
        content_type = self._get_content_type(ext)
        
        # Upload to MinIO
        self.client.fput_object(
            self.bucket,
            object_name,
            local_path,
            content_type=content_type
        )
        
        # Generate URL
        url = f"http://{self.settings.minio_endpoint}/{self.bucket}/{object_name}"
        
        logger.info(f"📤 Đã upload: {object_name}")
        return url
    
    def _get_content_type(self, ext: str) -> str:
        """Get content type based on file extension."""
        content_types = {
            ".mp4": "video/mp4",
            ".webm": "video/webm",
            ".png": "image/png",
            ".jpg": "image/jpeg",
            ".jpeg": "image/jpeg",
            ".gif": "image/gif",
        }
        return content_types.get(ext.lower(), "application/octet-stream")

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
        
        # Try to detect if it's an internal MinIO URL that requires authentication
        is_internal_minio = False
        minio_bucket = None
        minio_object = None
        
        try:
            from urllib.parse import urlparse, unquote
            parsed = urlparse(url)
            
            # Check if host matches MinIO endpoint (e.g. minio:9000)
            if self.settings.minio_endpoint in parsed.netloc:
                # Path usually starts with /bucket/object/key...
                path_parts = parsed.path.strip("/").split("/", 1)
                if len(path_parts) == 2:
                    minio_bucket = path_parts[0]
                    minio_object = unquote(path_parts[1])
                    is_internal_minio = True
                    logger.info(f"🔍 Detected internal MinIO URL: bucket={minio_bucket}, object={minio_object}")
        except Exception as e:
            logger.warning(f"⚠️ Failed to parse URL for MinIO detection: {e}")

        if is_internal_minio:
            try:
                # Use MinIO SDK (authenticated)
                self.client.fget_object(minio_bucket, minio_object, local_path)
                logger.info(f"📥 Đã tải xuống (via SDK): {url}")
                return local_path
            except Exception as e:
                logger.warning(f"⚠️ SDK download failed, falling back to HTTP: {e}")
                # Fallback to HTTP if SDK fails
        
        # Download from URL (works for both external, internal, and public URLs via HTTP)
        async with aiohttp.ClientSession() as session:
            try:
                async with session.get(url) as response:
                    if response.status == 200:
                        async with aiofiles.open(local_path, 'wb') as f:
                            async for chunk in response.content.iter_chunked(8192):
                                await f.write(chunk)
                        logger.info(f"📥 Đã tải xuống (via HTTP): {url}")
                    else:
                        error_msg = f"Failed to download: {url}, status: {response.status}"
                        logger.error(error_msg)
                        raise Exception(error_msg)
            except Exception as e:
                logger.error(f"❌ Download error: {e}")
                raise
        
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

"""
Trợ Lý KOC - AI Worker
Python service that consumes job requests from RabbitMQ and processes them using AI models.

Supported Job Types:
1. TalkingHead (LivePortrait)
2. VirtualTryOn (IDM-VTON)
3. ImageToVideo (SVD-XT)
4. MotionTransfer (MimicMotion)
5. FaceSwap (FaceFusion)
"""

import asyncio
import logging
import os
from dotenv import load_dotenv

from worker.message_consumer import MessageConsumer
from worker.config import Settings

# Load environment variables
load_dotenv()

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s | %(levelname)s | %(name)s | %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)
logger = logging.getLogger(__name__)


async def main():
    """Main entry point for the AI Worker."""
    settings = Settings()
    
    logger.info("=" * 60)
    logger.info("🚀 Khởi động Trợ Lý KOC AI Worker")
    logger.info(f"📡 RabbitMQ: {settings.rabbitmq_host}")
    logger.info(f"💾 MinIO: {settings.minio_endpoint}")
    logger.info("=" * 60)
    
    consumer = MessageConsumer(settings)
    
    try:
        await consumer.start()
    except KeyboardInterrupt:
        logger.info("⛔ Đang dừng worker...")
    finally:
        await consumer.stop()
        logger.info("👋 Worker đã dừng")


if __name__ == "__main__":
    asyncio.run(main())

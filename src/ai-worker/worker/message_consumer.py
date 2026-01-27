"""
RabbitMQ Message Consumer
Connects to RabbitMQ and consumes job requests from MassTransit.
"""

import json
import logging
import asyncio
from typing import Dict, Any
from aio_pika import connect_robust, Message, IncomingMessage, ExchangeType
from aio_pika.abc import AbstractRobustConnection, AbstractChannel

from worker.config import Settings, QUEUE_NAMES
from worker.job_dispatcher import JobDispatcher

logger = logging.getLogger(__name__)

# MassTransit routing keys (message type URN format)
MASSTRANSIT_ROUTING_KEYS = {
    "TalkingHead": "TroLiKOC.Modules.Jobs.Contracts.Messages:TalkingHeadRequest",
    "VirtualTryOn": "TroLiKOC.Modules.Jobs.Contracts.Messages:VirtualTryOnRequest",
    "ImageToVideo": "TroLiKOC.Modules.Jobs.Contracts.Messages:ImageToVideoRequest",
    "MotionTransfer": "TroLiKOC.Modules.Jobs.Contracts.Messages:MotionTransferRequest",
    "FaceSwap": "TroLiKOC.Modules.Jobs.Contracts.Messages:FaceSwapRequest",
}


class MessageConsumer:
    """RabbitMQ consumer that processes AI job requests from MassTransit."""
    
    def __init__(self, settings: Settings):
        self.settings = settings
        self.connection: AbstractRobustConnection = None
        self.channel: AbstractChannel = None
        self.dispatcher = JobDispatcher(settings)
        self._running = False
    
    async def start(self):
        """Start consuming messages from job-requests exchange."""
        # Connect to RabbitMQ
        connection_url = (
            f"amqp://{self.settings.rabbitmq_user}:{self.settings.rabbitmq_pass}"
            f"@{self.settings.rabbitmq_host}:{self.settings.rabbitmq_port}"
            f"{self.settings.rabbitmq_vhost}"
        )
        
        logger.info(f"🔌 Đang kết nối RabbitMQ: {self.settings.rabbitmq_host}...")
        self.connection = await connect_robust(connection_url)
        self.channel = await self.connection.channel()
        
        # Set QoS (prefetch count)
        await self.channel.set_qos(prefetch_count=self.settings.prefetch_count)
        
        # Declare the job-requests exchange (topic type - same as MassTransit config)
        job_exchange = await self.channel.declare_exchange(
            "job-requests",
            ExchangeType.TOPIC,
            durable=True
        )
        logger.info("📡 Đã kết nối exchange: job-requests (topic)")
        
        # Declare completion exchange for sending results back
        self.completion_exchange = await self.channel.declare_exchange(
            "job-completions",
            ExchangeType.FANOUT,
            durable=True
        )
        
        # For each job type, create queue and bind with correct routing key
        for job_type, queue_name in QUEUE_NAMES.items():
            # Declare our queue
            queue = await self.channel.declare_queue(queue_name, durable=True)
            
            # Get MassTransit routing key for this job type
            mt_routing_key = MASSTRANSIT_ROUTING_KEYS[job_type]
            
            # Bind queue to job-requests exchange with MassTransit routing key
            await queue.bind(job_exchange, routing_key=mt_routing_key)
            logger.info(f"🔗 Đã bind {queue_name} với routing key: {mt_routing_key}")
            
            # Start consuming with job_type context
            await queue.consume(
                lambda msg, jt=job_type: self._on_message(msg, jt)
            )
            logger.info(f"📥 Đang lắng nghe queue: {queue_name}")
        
        self._running = True
        logger.info("✅ Worker sẵn sàng nhận công việc!")
        
        # Keep running
        while self._running:
            await asyncio.sleep(1)
    
    async def stop(self):
        """Stop consuming and close connections."""
        self._running = False
        if self.channel:
            await self.channel.close()
        if self.connection:
            await self.connection.close()
    
    async def _on_message(self, message: IncomingMessage, job_type: str):
        """Handle incoming job request."""
        logger.info(f"🔍 Nhận message! routing_key={message.routing_key}, job_type={job_type}")
        
        async with message.process():
            try:
                # Parse message body
                raw_body = message.body.decode()
                logger.info(f"📦 Raw message (first 500 chars): {raw_body[:500]}")
                
                body = json.loads(raw_body)
                
                # MassTransit envelope format: { "message": { ... actual payload ... } }
                if "message" in body:
                    payload = body["message"]
                    logger.info("📋 Detected MassTransit envelope format")
                else:
                    payload = body
                
                # Extract job info (MassTransit uses camelCase in JSON)
                job_id = payload.get("jobId") or payload.get("JobId")
                
                # Try to detect actual job type from routing key or messageType
                detected_type = self._detect_job_type(message.routing_key, body)
                if detected_type != "Unknown":
                    job_type = detected_type
                
                logger.info(f"📨 Processing Job {job_id} type {job_type}")
                
                # Process the job
                result = await self.dispatcher.dispatch(job_type, payload)
                
                # Publish completion event
                await self._publish_completion(job_id, result)
                
                logger.info(f"✅ Hoàn thành Job {job_id}")
                
            except Exception as e:
                logger.error(f"❌ Lỗi xử lý message: {e}", exc_info=True)
                
                # Try to publish failure
                try:
                    if 'payload' in dir() and payload:
                        job_id = payload.get("jobId") or payload.get("JobId")
                        if job_id:
                            await self._publish_completion(job_id, {
                                "status": "FAILED",
                                "error": str(e)
                            })
                except Exception as pub_err:
                    logger.error(f"Failed to publish error: {pub_err}")
    
    def _detect_job_type(self, routing_key: str, body: Dict[str, Any]) -> str:
        """Detect job type from routing key or message body."""
        rk = routing_key or ""
        
        # Check routing key patterns
        if "TalkingHead" in rk:
            return "TalkingHead"
        elif "VirtualTryOn" in rk:
            return "VirtualTryOn"
        elif "ImageToVideo" in rk:
            return "ImageToVideo"
        elif "MotionTransfer" in rk:
            return "MotionTransfer"
        elif "FaceSwap" in rk:
            return "FaceSwap"
        
        # Fallback: check messageType in MassTransit envelope
        msg_types = body.get("messageType", [])
        for t in msg_types:
            if "TalkingHead" in t:
                return "TalkingHead"
            elif "VirtualTryOn" in t:
                return "VirtualTryOn"
            elif "ImageToVideo" in t:
                return "ImageToVideo"
            elif "MotionTransfer" in t:
                return "MotionTransfer"
            elif "FaceSwap" in t:
                return "FaceSwap"
        
        return "Unknown"
    
    async def _publish_completion(self, job_id: str, result: Dict[str, Any]):
        """Publish job completion event to RabbitMQ."""
        completion_message = {
            "jobId": str(job_id),
            "status": result.get("status", "COMPLETED"),
            "outputUrl": result.get("output_url"),
            "error": result.get("error"),
            "processingTimeMs": result.get("processing_time_ms", 0)
        }
        
        message = Message(
            body=json.dumps(completion_message).encode(),
            content_type="application/json"
        )
        
        await self.completion_exchange.publish(message, routing_key="")
        logger.info(f"📤 Đã gửi kết quả Job {job_id}")

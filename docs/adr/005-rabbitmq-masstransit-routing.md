# ADR 005: RabbitMQ Message Routing Between MassTransit and Python Worker

## Status
**Accepted** - Implemented on 2026-01-27

## Context

The Trợ Lý KOC platform uses a **polyglot architecture** where:
- **Backend API** (.NET 10) uses **MassTransit** for message publishing
- **AI Worker** (Python) uses **aio-pika** for message consumption

During integration testing, we discovered that the AI Worker was **not receiving job messages** from the API, despite:
- API successfully publishing messages to RabbitMQ (`publish_in: 4` on exchange)
- AI Worker connecting to RabbitMQ and binding queues
- Both services using the same exchange name `job-requests`

## Problem Analysis

### Root Cause: Routing Key Mismatch

MassTransit and raw AMQP clients use different conventions for message routing:

| Aspect | MassTransit (.NET) | aio-pika (Python) Initial Setup |
|--------|-------------------|-------------------------------|
| Exchange Type | Topic (configured) | Topic |
| Routing Key | Full message type URN | Simple keys like `job.talkinghead` |
| Example | `TroLiKOC.Modules.Jobs.Contracts.Messages:TalkingHeadRequest` | `job.talking-head` |

### MassTransit Behavior

When you call `await _publishEndpoint.Publish(new TalkingHeadRequest { ... })`, MassTransit:

1. Sends to exchange named `job-requests` (because we configured `SetEntityName("job-requests")`)
2. Uses the **full message type URN** as the routing key:
   ```
   TroLiKOC.Modules.Jobs.Contracts.Messages:TalkingHeadRequest
   ```

### Original Python Worker Binding

```python
# ❌ WRONG: These routing keys don't match MassTransit's convention
await queue.bind(exchange, routing_key="job.talking-head")
await queue.bind(exchange, routing_key="TroLiKOC.Modules.Jobs:TalkingHeadRequest")  # Missing .Contracts.Messages
```

## Decision

### Solution: Align Python Worker with MassTransit Routing Keys

Update `message_consumer.py` to:

1. **Bind to the correct exchange**: `job-requests` (topic type)
2. **Use exact MassTransit routing keys**:

```python
MASSTRANSIT_ROUTING_KEYS = {
    "TalkingHead": "TroLiKOC.Modules.Jobs.Contracts.Messages:TalkingHeadRequest",
    "VirtualTryOn": "TroLiKOC.Modules.Jobs.Contracts.Messages:VirtualTryOnRequest",
    "ImageToVideo": "TroLiKOC.Modules.Jobs.Contracts.Messages:ImageToVideoRequest",
    "MotionTransfer": "TroLiKOC.Modules.Jobs.Contracts.Messages:MotionTransferRequest",
    "FaceSwap": "TroLiKOC.Modules.Jobs.Contracts.Messages:FaceSwapRequest",
}

# Correct binding
for job_type, queue_name in QUEUE_NAMES.items():
    queue = await channel.declare_queue(queue_name, durable=True)
    mt_routing_key = MASSTRANSIT_ROUTING_KEYS[job_type]
    await queue.bind(job_exchange, routing_key=mt_routing_key)
```

### MassTransit Configuration (.NET API)

```csharp
// Program.cs - These settings make MassTransit publish to job-requests exchange
cfg.Message<TalkingHeadRequest>(m => m.SetEntityName("job-requests"));
cfg.Publish<TalkingHeadRequest>(p => p.ExchangeType = "topic");
// ... same for other request types
```

## Consequences

### Positive
- ✅ AI Worker now correctly receives all job messages
- ✅ Each job type is routed to its dedicated queue
- ✅ No duplicate message processing (fixed wildcard binding issue)
- ✅ Clear documentation of routing convention

### Negative
- ⚠️ Tight coupling to MassTransit's message type naming convention
- ⚠️ Python Worker must be updated if .NET message types are renamed/moved

### Mitigations
- Document the routing key format requirement
- Consider using a shared configuration file for routing keys
- Add integration tests that verify message routing end-to-end

## Alternative Considered: Use Fanout Exchanges

We could configure MassTransit to use **fanout exchanges per message type** instead of a shared topic exchange:

```csharp
// Alternative: Let MassTransit create separate exchanges
cfg.Message<TalkingHeadRequest>(m => m.SetEntityName("TalkingHeadRequest"));
cfg.Publish<TalkingHeadRequest>(p => p.ExchangeType = "fanout");
```

**Why rejected:**
- Would require AI Worker to bind to 5+ different exchanges
- Harder to add new job types
- Shared topic exchange is cleaner for this use case

## Message Format

MassTransit wraps messages in an envelope:

```json
{
  "messageId": "guid",
  "conversationId": "guid",
  "messageType": [
    "urn:message:TroLiKOC.Modules.Jobs.Contracts.Messages:TalkingHeadRequest"
  ],
  "message": {
    "jobId": "guid",
    "userId": "guid",
    "sourceImageUrl": "https://...",
    "audioUrl": "https://...",
    "priority": "High",
    "outputResolution": "1080p",
    "addWatermark": true,
    "createdAt": "2026-01-27T00:00:00Z"
  },
  "sentTime": "2026-01-27T00:00:00Z",
  "headers": {},
  "host": { ... }
}
```

Python Worker extracts the actual payload:

```python
body = json.loads(message.body.decode())
if "message" in body:
    payload = body["message"]  # Extract from MassTransit envelope
else:
    payload = body
```

## Debugging Tips

### Check RabbitMQ Exchanges
```powershell
Invoke-RestMethod -Uri "http://localhost:15672/api/exchanges" `
    -Headers @{Authorization=("Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123")))}
```

### Check Queue Bindings
```powershell
Invoke-RestMethod -Uri "http://localhost:15672/api/bindings" `
    -Headers @{Authorization=("Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123")))}
```

### Verify Message Flow
1. API logs: `Đang gửi Job {JobId} loại {JobType} tới RabbitMQ`
2. RabbitMQ: `job-requests` exchange should show `publish_in` count increasing
3. AI Worker logs: `Nhận message! routing_key=..., job_type=...`

## Related ADRs
- [ADR 002: MassTransit Version](002-masstransit-version.md)
- [ADR 004: AI Worker Architecture](004-ai-worker-architecture.md)

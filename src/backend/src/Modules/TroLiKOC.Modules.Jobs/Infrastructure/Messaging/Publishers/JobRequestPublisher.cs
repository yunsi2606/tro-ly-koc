using MassTransit;
using Microsoft.Extensions.Logging;
using TroLiKOC.Modules.Jobs.Contracts.Messages;
using TroLiKOC.Modules.Jobs.Domain.Enums;
using System.Text.Json;

namespace TroLiKOC.Modules.Jobs.Infrastructure.Messaging.Publishers;

public interface IJobRequestPublisher
{
    Task PublishJobRequestAsync(Guid jobId, Guid userId, JobType jobType, string priority, string inputPayload);
}

public class JobRequestPublisher : IJobRequestPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<JobRequestPublisher> _logger;

    public JobRequestPublisher(IPublishEndpoint publishEndpoint, ILogger<JobRequestPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishJobRequestAsync(Guid jobId, Guid userId, JobType jobType, string priority, string inputPayload)
    {
        _logger.LogInformation("Đang gửi Job {JobId} loại {JobType} tới RabbitMQ", jobId, jobType);

        switch (jobType)
        {
            case JobType.TalkingHead:
                var thPayload = JsonSerializer.Deserialize<TalkingHeadPayload>(inputPayload);
                await _publishEndpoint.Publish(new TalkingHeadRequest
                {
                    JobId = jobId,
                    UserId = userId,
                    SourceImageUrl = thPayload?.SourceImageUrl ?? "",
                    AudioUrl = thPayload?.AudioUrl ?? "",
                    Priority = priority,
                    OutputResolution = thPayload?.OutputResolution ?? "720p",
                    AddWatermark = thPayload?.AddWatermark ?? true
                });
                break;

            case JobType.VirtualTryOn:
                var vtoPayload = JsonSerializer.Deserialize<VirtualTryOnPayload>(inputPayload);
                await _publishEndpoint.Publish(new VirtualTryOnRequest
                {
                    JobId = jobId,
                    UserId = userId,
                    ModelImageUrl = vtoPayload?.ModelImageUrl ?? "",
                    GarmentImageUrl = vtoPayload?.GarmentImageUrl ?? "",
                    Priority = priority,
                    OutputResolution = vtoPayload?.OutputResolution ?? "720p"
                });
                break;

            case JobType.ImageToVideo:
                var i2vPayload = JsonSerializer.Deserialize<ImageToVideoPayload>(inputPayload);
                await _publishEndpoint.Publish(new ImageToVideoRequest
                {
                    JobId = jobId,
                    UserId = userId,
                    SourceImageUrl = i2vPayload?.SourceImageUrl ?? "",
                    Priority = priority,
                    OutputResolution = i2vPayload?.OutputResolution ?? "720p"
                });
                break;

            case JobType.MotionTransfer:
                var mtPayload = JsonSerializer.Deserialize<MotionTransferPayload>(inputPayload);
                await _publishEndpoint.Publish(new MotionTransferRequest
                {
                    JobId = jobId,
                    UserId = userId,
                    SourceImageUrl = mtPayload?.SourceImageUrl ?? "",
                    SkeletonVideoUrl = mtPayload?.SkeletonVideoUrl ?? "",
                    Priority = priority
                });
                break;

            case JobType.FaceSwap:
                var fsPayload = JsonSerializer.Deserialize<FaceSwapPayload>(inputPayload);
                await _publishEndpoint.Publish(new FaceSwapRequest
                {
                    JobId = jobId,
                    UserId = userId,
                    SourceVideoUrl = fsPayload?.SourceVideoUrl ?? "",
                    TargetFaceUrl = fsPayload?.TargetFaceUrl ?? "",
                    Priority = priority
                });
                break;
        }

        _logger.LogInformation("Đã gửi Job {JobId} thành công", jobId);
    }
}

// Internal payload DTOs for deserialization
internal record TalkingHeadPayload(string SourceImageUrl, string AudioUrl, string? OutputResolution, bool? AddWatermark);
internal record VirtualTryOnPayload(string ModelImageUrl, string GarmentImageUrl, string? OutputResolution);
internal record ImageToVideoPayload(string SourceImageUrl, string? OutputResolution);
internal record MotionTransferPayload(string SourceImageUrl, string SkeletonVideoUrl);
internal record FaceSwapPayload(string SourceVideoUrl, string TargetFaceUrl);

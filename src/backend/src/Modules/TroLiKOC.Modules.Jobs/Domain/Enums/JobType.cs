namespace TroLiKOC.Modules.Jobs.Domain.Enums;

public enum JobType
{
    TalkingHead,
    VirtualTryOn,
    ImageToVideo,
    MotionTransfer,
    FaceSwap
}

public enum JobStatus
{
    Pending,
    Queued,
    Processing,
    Completed,
    Failed
}

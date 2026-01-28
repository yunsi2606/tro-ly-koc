using TroLiKOC.Modules.Jobs.Contracts.Messages;

namespace TroLiKOC.Modules.Jobs.Contracts;

public interface IJobNotifier
{
    Task NotifyJobCompletedAsync(Guid userId, JobCompletedEvent jobResult);
    Task NotifyJobFailedAsync(Guid userId, Guid jobId, string error);
}

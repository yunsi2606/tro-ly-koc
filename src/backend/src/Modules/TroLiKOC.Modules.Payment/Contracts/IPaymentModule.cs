namespace TroLiKOC.Modules.Payment.Contracts;

public interface IPaymentModule
{
    Task<Guid> LogWebhookAsync(SePayWebhookDto dto);
    Task MarkWebhookProcessedAsync(Guid logId, Guid userId);
    Task MarkWebhookFailedAsync(Guid logId, string error);
}

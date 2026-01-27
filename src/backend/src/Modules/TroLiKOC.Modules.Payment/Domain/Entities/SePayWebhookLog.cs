using TroLiKOC.SharedKernel.Domain;

namespace TroLiKOC.Modules.Payment.Domain.Entities;

public class SePayWebhookLog : BaseEntity
{
    public string TransactionId { get; private set; }
    public string RawPayload { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public Guid? ProcessedUserId { get; private set; }
    public string Status { get; private set; } = "RECEIVED"; // RECEIVED, PROCESSED, FAILED
    public string? ErrorMessage { get; private set; }
    public DateTime ReceivedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }

    internal SePayWebhookLog(string transactionId, string rawPayload, decimal amount, string? description)
    {
        TransactionId = transactionId;
        RawPayload = rawPayload;
        Amount = amount;
        Description = description;
    }

    public void MarkProcessed(Guid userId)
    {
        ProcessedUserId = userId;
        Status = "PROCESSED";
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = "FAILED";
        ErrorMessage = error;
        ProcessedAt = DateTime.UtcNow;
    }

    // EF Core
    private SePayWebhookLog() { }
}

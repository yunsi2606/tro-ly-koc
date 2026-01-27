using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Payment.Contracts;
using TroLiKOC.Modules.Payment.Domain.Entities;
using TroLiKOC.Modules.Payment.Infrastructure;

namespace TroLiKOC.Modules.Payment.Services;

public class PaymentModuleService : IPaymentModule
{
    private readonly PaymentDbContext _dbContext;

    public PaymentModuleService(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> LogWebhookAsync(SePayWebhookDto dto)
    {
        // Check if transaction already exists
        var existingLog = await _dbContext.Set<SePayWebhookLog>()
            .FirstOrDefaultAsync(x => x.TransactionId == dto.Id.ToString());

        if (existingLog != null)
        {
            return existingLog.Id;
        }

        var rawPayload = JsonSerializer.Serialize(dto);
        var log = new SePayWebhookLog(
            dto.Id.ToString(),
            rawPayload,
            dto.TransferAmount,
            dto.Content
        );

        _dbContext.Add(log);
        await _dbContext.SaveChangesAsync();

        return log.Id;
    }

    public async Task MarkWebhookProcessedAsync(Guid logId, Guid userId)
    {
        var log = await _dbContext.Set<SePayWebhookLog>().FindAsync(logId);
        if (log != null)
        {
            log.MarkProcessed(userId);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task MarkWebhookFailedAsync(Guid logId, string error)
    {
        var log = await _dbContext.Set<SePayWebhookLog>().FindAsync(logId);
        if (log != null)
        {
            log.MarkFailed(error);
            await _dbContext.SaveChangesAsync();
        }
    }
}

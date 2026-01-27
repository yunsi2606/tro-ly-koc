using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Wallet.Contracts;
using TroLiKOC.Modules.Wallet.Infrastructure;

namespace TroLiKOC.Modules.Wallet.Application.Services;

public class WalletService : IWalletModule
{
    private readonly WalletDbContext _dbContext;

    public WalletService(WalletDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WalletDto> GetByUserIdAsync(Guid userId)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) return null; // Or throw

        return new WalletDto(wallet.Id, wallet.UserId, wallet.Balance, wallet.Currency);
    }

    public async Task CreateWalletAsync(Guid userId)
    {
        var exists = await _dbContext.Wallets.AnyAsync(w => w.UserId == userId);
        if (exists) return;

        var wallet = new Domain.Entities.Wallet(userId);
        _dbContext.Wallets.Add(wallet);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<WalletDto> TopUpAsync(Guid userId, decimal amount, string reference, string description = "Nạp tiền")
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) throw new InvalidOperationException("Wallet not found");

        wallet.TopUp(amount, reference, description);
        await _dbContext.SaveChangesAsync();

        return new WalletDto(wallet.Id, wallet.UserId, wallet.Balance, wallet.Currency);
    }

    public async Task<WalletDto> DeductAsync(Guid userId, decimal amount, string description)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) throw new InvalidOperationException("Wallet not found");

        var success = wallet.Deduct(amount, description);
        if (!success) throw new InvalidOperationException("Insufficient balance");

        await _dbContext.SaveChangesAsync();

        return new WalletDto(wallet.Id, wallet.UserId, wallet.Balance, wallet.Currency);
    }

    public async Task<IReadOnlyList<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int size)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) return Array.Empty<TransactionDto>();

        return await _dbContext.Transactions
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new TransactionDto(
                t.Id, t.Type, t.Amount, t.BalanceBefore, t.BalanceAfter, t.Reference, t.Description, t.Status, t.CreatedAt))
            .ToListAsync();
    }
}

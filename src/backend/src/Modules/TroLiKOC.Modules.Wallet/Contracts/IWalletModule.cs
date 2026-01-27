namespace TroLiKOC.Modules.Wallet.Contracts;

public interface IWalletModule
{
    Task<WalletDto> GetByUserIdAsync(Guid userId);
    Task<WalletDto> TopUpAsync(Guid userId, decimal amount, string reference, string description = "Nạp tiền");
    Task<WalletDto> DeductAsync(Guid userId, decimal amount, string description);
    Task<IReadOnlyList<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int size);
    Task CreateWalletAsync(Guid userId);
}

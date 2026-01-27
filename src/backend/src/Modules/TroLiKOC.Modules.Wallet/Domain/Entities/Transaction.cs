using TroLiKOC.SharedKernel.Domain;

namespace TroLiKOC.Modules.Wallet.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid WalletId { get; private set; }
    public string Type { get; private set; } // DEPOSIT, PAYMENT, REFUND
    public decimal Amount { get; private set; }
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string? Reference { get; private set; }
    public string? Description { get; private set; }
    public string Status { get; private set; } = "COMPLETED";

    internal Transaction(Guid walletId, string type, decimal amount, decimal balanceBefore, decimal balanceAfter, string? reference, string? description)
    {
        WalletId = walletId;
        Type = type;
        Amount = amount;
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
        Reference = reference;
        Description = description;
    }

    // EF Core
    private Transaction() { }
}

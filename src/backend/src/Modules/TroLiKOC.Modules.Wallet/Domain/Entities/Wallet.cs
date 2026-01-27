using TroLiKOC.SharedKernel.Domain;

namespace TroLiKOC.Modules.Wallet.Domain.Entities;

public class Wallet : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public string Currency { get; private set; } = "VND";
    
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public Wallet(Guid userId)
    {
        UserId = userId;
        Balance = 0;
    }

    public void TopUp(decimal amount, string reference, string description)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        var balanceBefore = Balance;
        Balance += amount;

        var transaction = new Transaction(Id, "DEPOSIT", amount, balanceBefore, Balance, reference, description);
        _transactions.Add(transaction);
        
        UpdateTimestamp();
    }

    public bool Deduct(decimal amount, string description)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
        if (Balance < amount) return false;

        var balanceBefore = Balance;
        Balance -= amount;

        var transaction = new Transaction(Id, "PAYMENT", -amount, balanceBefore, Balance, null, description);
        _transactions.Add(transaction);
        
        UpdateTimestamp();
        return true;
    }
}

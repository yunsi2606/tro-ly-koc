namespace TroLiKOC.Modules.Wallet.Contracts;

public record WalletDto(Guid Id, Guid UserId, decimal Balance, string Currency);

public record TransactionDto(
    Guid Id, 
    string Type, 
    decimal Amount, 
    decimal BalanceBefore, 
    decimal BalanceAfter, 
    string? Reference, 
    string? Description, 
    string Status, 
    DateTime CreatedAt);

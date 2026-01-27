using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroLiKOC.Modules.Wallet.Contracts;
using System.Security.Claims;

namespace TroLiKOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletModule _walletModule;

    public WalletController(IWalletModule walletModule)
    {
        _walletModule = walletModule;
    }

    /// <summary>
    /// Lấy thông tin ví của người dùng hiện tại
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<WalletDto>> GetMyWallet()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString());
        var wallet = await _walletModule.GetByUserIdAsync(userId);
        
        if (wallet == null)
        {
            await _walletModule.CreateWalletAsync(userId);
            wallet = await _walletModule.GetByUserIdAsync(userId);
        }

        return Ok(wallet);
    }

    /// <summary>
    /// Lấy lịch sử giao dịch
    /// </summary>
    [HttpGet("transactions")]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString());
        var transactions = await _walletModule.GetTransactionsAsync(userId, page, size);
        return Ok(transactions);
    }

    /// <summary>
    /// Tạo yêu cầu nạp tiền - Trả về thông tin thanh toán
    /// </summary>
    [HttpPost("create-payment")]
    [Authorize]
    public async Task<ActionResult<CreatePaymentResponse>> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        
        // Validation
        if (request.Amount < 10000)
            return BadRequest(new { message = "Số tiền tối thiểu là 10,000 VNĐ" });
        if (request.Amount > 50000000)
            return BadRequest(new { message = "Số tiền tối đa là 50,000,000 VNĐ" });

        // Get SePay configuration
        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var bankAccount = config["SePay:BankAccount"] ?? "0441000726088";
        var bankName = config["SePay:BankName"] ?? "Vietcombank";
        var accountName = config["SePay:AccountName"] ?? "TROLYKOC";
        var prefix = config["SePay:Prefix"] ?? "TROLIKOC";

        // Generate payment content
        var content = $"{prefix} {userId}";
        
        // Generate VietQR URL
        // Format: https://img.vietqr.io/image/<BANK_ID>-<ACCOUNT_NO>-<TEMPLATE>.png?amount=<AMOUNT>&addInfo=<CONTENT>&accountName=<NAME>
        var vietQrUrl = $"https://img.vietqr.io/image/VCB-{bankAccount}-qr_only.png?amount={request.Amount}&addInfo={Uri.EscapeDataString(content)}&accountName={Uri.EscapeDataString(accountName)}";

        return Ok(new CreatePaymentResponse
        {
            BankAccount = bankAccount,
            BankName = bankName,
            AccountName = accountName,
            Amount = request.Amount,
            Content = content,
            QrCodeUrl = vietQrUrl,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });
    }

    /// <summary>
    /// Nạp tiền vào ví (Internal - được gọi từ Payment webhook)
    /// </summary>
    [HttpPost("topup")]
    // [Authorize(Roles = "System")]
    public async Task<ActionResult<WalletDto>> TopUp([FromBody] TopUpRequest request)
    {
        var wallet = await _walletModule.TopUpAsync(
            request.UserId,
            request.Amount,
            request.Reference,
            request.Description ?? "Nạp tiền");

        return Ok(wallet);
    }
}

public record CreatePaymentRequest(decimal Amount);
public record CreatePaymentResponse
{
    public required string BankAccount { get; init; }
    public required string BankName { get; init; }
    public required string AccountName { get; init; }
    public decimal Amount { get; init; }
    public required string Content { get; init; }
    public required string QrCodeUrl { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public record TopUpRequest(Guid UserId, decimal Amount, string Reference, string? Description);

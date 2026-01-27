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

public record TopUpRequest(Guid UserId, decimal Amount, string Reference, string? Description);

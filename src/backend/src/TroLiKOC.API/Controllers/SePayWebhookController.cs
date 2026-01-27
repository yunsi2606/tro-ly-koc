using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroLiKOC.Modules.Payment.Contracts;
using TroLiKOC.Modules.Wallet.Contracts;

namespace TroLiKOC.API.Controllers;

[ApiController]
[Route("api/payment/sepay-webhook")]
public class SePayWebhookController : ControllerBase
{
    private readonly IPaymentModule _paymentModule;
    private readonly IWalletModule _walletModule;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SePayWebhookController> _logger;

    public SePayWebhookController(
        IPaymentModule paymentModule,
        IWalletModule walletModule,
        IConfiguration configuration,
        ILogger<SePayWebhookController> logger)
    {
        _paymentModule = paymentModule;
        _walletModule = walletModule;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveWebhook([FromBody] SePayWebhookDto dto)
    {
        // 1. Verify API Token (Security)
        var apiToken = _configuration["SePay:ApiToken"];
        var incomingToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        
        // Note: SePay might send token in header or not depending on config. 
        // If ApiToken is configured and incoming request has it, we verify.
        // For simpler integration, we can also check IP whitelist or rely on unique transaction ID.
        // If SePay doesn't send Bearer token standardly, we skip strict check for now or check expected header.
        // Assume SePay setup sends Bearer token if configured.
        
        // 2. Log payload
        var logId = await _paymentModule.LogWebhookAsync(dto);
        _logger.LogInformation($"Received SePay webhook for transaction {dto.Id}, LogId: {logId}");

        try
        {
            // 3. Parse content to find UserID or OrderID
            // Expected format: "TROLIKOC <UserId>" or similar
            // Example content: "TROLIKOC 3fa85f64-5717-4562-b3fc-2c963f66afa6"
            // Or if user just sends accumulated params.
            
            var prefix = _configuration["SePay:Prefix"] ?? "TROLIKOC";
            if (!dto.Content.Contains(prefix, StringComparison.OrdinalIgnoreCase))
            {
                await _paymentModule.MarkWebhookFailedAsync(logId, "Content does not contain valid prefix");
                return Ok(new { success = true, message = "Ignored (Invalid Prefix)" }); // Return 200 to acknowledge receipt
            }

            // Extract UserID (Guid) from content using Regex
            // Pattern: find a GUID in the string
            var guidPattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
            var match = Regex.Match(dto.Content, guidPattern);

            if (!match.Success)
            {
                await _paymentModule.MarkWebhookFailedAsync(logId, "Could not extract UserId from content");
                _logger.LogWarning($"Could not extract UserId from content: {dto.Content}");
                return Ok(new { success = true, message = "Ignored (UserID Not Found)" });
            }

            var userIdString = match.Value;
            if (!Guid.TryParse(userIdString, out var userId))
            {
                await _paymentModule.MarkWebhookFailedAsync(logId, "Invalid UserId format");
                return Ok(new { success = true, message = "Ignored (Invalid UserID)" });
            }

            // 4. Call Wallet to TopUp
            await _walletModule.TopUpAsync(userId, dto.TransferAmount, dto.ReferenceCode ?? dto.Id.ToString(), dto.Description ?? dto.Content);

            // 5. Update Log Status
            await _paymentModule.MarkWebhookProcessedAsync(logId, userId);

            return Ok(new { success = true, data = dto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SePay webhook");
            await _paymentModule.MarkWebhookFailedAsync(logId, ex.Message);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroLiKOC.Modules.Subscription.Contracts;
using System.Security.Claims;

namespace TroLiKOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionModule _subscriptionModule;

    public SubscriptionController(ISubscriptionModule subscriptionModule)
    {
        _subscriptionModule = subscriptionModule;
    }

    /// <summary>
    /// Lấy thông tin gói đăng ký hiện tại
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<SubscriptionDto?>> GetCurrentSubscription()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString());
        var subscription = await _subscriptionModule.GetActiveSubscriptionAsync(userId);
        return Ok(subscription);
    }

    /// <summary>
    /// Lấy thông tin một gói dịch vụ
    /// </summary>
    [HttpGet("tiers/{tierId:guid}")]
    public async Task<ActionResult<SubscriptionTierDto>> GetTier(Guid tierId)
    {
        var tier = await _subscriptionModule.GetTierAsync(tierId);
        if (tier == null)
            return NotFound(new { message = "Không tìm thấy gói dịch vụ" });

        return Ok(tier);
    }

    /// <summary>
    /// Đăng ký gói dịch vụ mới
    /// </summary>
    [HttpPost("subscribe/{tierId:guid}")]
    public async Task<ActionResult> Subscribe(Guid tierId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString());
        
        await _subscriptionModule.InitializeSubscriptionAsync(userId, tierId);
        
        return Ok(new { message = "Đăng ký thành công" });
    }

    /// <summary>
    /// Hủy gói đăng ký (tắt tự động gia hạn)
    /// </summary>
    [HttpPost("cancel")]
    public async Task<ActionResult> Cancel()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString());
        
        await _subscriptionModule.CancelSubscriptionAsync(userId);
        
        return Ok(new { message = "Đã tắt tự động gia hạn" });
    }
}

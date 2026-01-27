using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroLiKOC.Modules.Jobs.Contracts;
using TroLiKOC.Modules.Jobs.Domain.Enums;
using TroLiKOC.Modules.Jobs.Infrastructure.Messaging.Publishers;
using System.Security.Claims;

namespace TroLiKOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // Uncomment when JWT is fully configured
public class JobsController : ControllerBase
{
    private readonly IJobsModule _jobsModule;
    private readonly IJobRequestPublisher _publisher;

    public JobsController(IJobsModule jobsModule, IJobRequestPublisher publisher)
    {
        _jobsModule = jobsModule;
        _publisher = publisher;
    }

    /// <summary>
    /// Tạo một công việc render mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RenderJobDto>> CreateJob([FromBody] CreateJobRequest request)
    {
        // TODO: Get actual userId from JWT claims
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString());

        var job = await _jobsModule.CreateJobAsync(
            userId,
            request.JobType,
            request.Priority,
            request.InputPayload);

        // Publish to RabbitMQ
        await _publisher.PublishJobRequestAsync(
            job.Id,
            userId,
            request.JobType,
            request.Priority,
            request.InputPayload);

        // Mark as queued
        await _jobsModule.MarkJobQueuedAsync(job.Id);

        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một công việc
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RenderJobDto>> GetJob(Guid id)
    {
        var job = await _jobsModule.GetJobAsync(id);
        if (job == null)
            return NotFound(new { message = "Không tìm thấy công việc" });

        return Ok(job);
    }

    /// <summary>
    /// Lấy danh sách công việc của người dùng
    /// </summary>
    [HttpGet("my-jobs")]
    public async Task<ActionResult<IReadOnlyList<RenderJobDto>>> GetMyJobs(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString());
        var jobs = await _jobsModule.GetUserJobsAsync(userId, page, size);
        return Ok(jobs);
    }

    /// <summary>
    /// Lấy danh sách công việc theo trạng thái (Admin)
    /// </summary>
    [HttpGet("by-status/{status}")]
    // [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<RenderJobDto>>> GetJobsByStatus(
        JobStatus status,
        [FromQuery] int limit = 50)
    {
        var jobs = await _jobsModule.GetJobsByStatusAsync(status, limit);
        return Ok(jobs);
    }
}

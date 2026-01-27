using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Jobs.Contracts;
using TroLiKOC.Modules.Jobs.Domain.Entities;
using TroLiKOC.Modules.Jobs.Domain.Enums;
using TroLiKOC.Modules.Jobs.Infrastructure;

namespace TroLiKOC.Modules.Jobs.Application.Services;

public class RenderJobService : IJobsModule
{
    private readonly JobsDbContext _dbContext;

    public RenderJobService(JobsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RenderJobDto> CreateJobAsync(Guid userId, JobType jobType, string priority, string inputPayload)
    {
        var job = new RenderJob(userId, jobType, priority, inputPayload);
        _dbContext.RenderJobs.Add(job);
        await _dbContext.SaveChangesAsync();
        return ToDto(job);
    }

    public async Task<RenderJobDto?> GetJobAsync(Guid jobId)
    {
        var job = await _dbContext.RenderJobs.FindAsync(jobId);
        return job == null ? null : ToDto(job);
    }

    public async Task<IReadOnlyList<RenderJobDto>> GetUserJobsAsync(Guid userId, int page, int size)
    {
        var jobs = await _dbContext.RenderJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return jobs.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<RenderJobDto>> GetJobsByStatusAsync(JobStatus status, int limit)
    {
        var jobs = await _dbContext.RenderJobs
            .Where(j => j.Status == status)
            .OrderBy(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return jobs.Select(ToDto).ToList();
    }

    public async Task MarkJobQueuedAsync(Guid jobId)
    {
        var job = await _dbContext.RenderJobs.FindAsync(jobId);
        if (job != null)
        {
            job.MarkQueued();
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task MarkJobStartedAsync(Guid jobId)
    {
        var job = await _dbContext.RenderJobs.FindAsync(jobId);
        if (job != null)
        {
            job.MarkStarted();
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task CompleteJobAsync(Guid jobId, string outputUrl, string outputKey, int processingTimeMs)
    {
        var job = await _dbContext.RenderJobs.FindAsync(jobId);
        if (job != null)
        {
            job.Complete(outputUrl, outputKey, processingTimeMs);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task FailJobAsync(Guid jobId, string error)
    {
        var job = await _dbContext.RenderJobs.FindAsync(jobId);
        if (job != null)
        {
            job.Fail(error);
            await _dbContext.SaveChangesAsync();
        }
    }

    private static RenderJobDto ToDto(RenderJob job)
    {
        return new RenderJobDto(
            job.Id,
            job.UserId,
            job.JobType,
            job.Status,
            job.Priority,
            job.OutputUrl,
            job.ErrorMessage,
            job.ProcessingTimeMs,
            job.CreatedAt,
            job.QueuedAt,
            job.StartedAt,
            job.CompletedAt);
    }
}

# ADR 006: Quartz.NET Job Store Configuration

## Status
**Accepted** - Implemented on 2026-01-27

## Context

Quartz.NET is used in the Trợ Lý KOC platform for scheduling recurring background jobs:
- **SubscriptionRenewalJob**: Runs daily at 00:05 VN time
- **SubscriptionExpiryJob**: Runs daily at 01:00 VN time  
- **NotificationBroadcastJob**: Runs every 5 minutes

The original implementation was configured to use **SQL Server Persistent Store** for job scheduling, which requires specific database tables to be created.

## Problem

During Docker deployment, the API container crashed on startup with error:

```
Error Number:208,State:1,Class:16
Invalid object name 'QRTZ_TRIGGERS'
```

This occurred because:
1. Quartz.NET Persistent Store requires specific tables (QRTZ_TRIGGERS, QRTZ_JOB_DETAILS, etc.)
2. These tables were **not created** by Entity Framework migrations (they're Quartz-specific)
3. The Quartz SQL scripts had not been run against the database

## Decision

### Short-term: Use In-Memory Store

For development and initial deployment, switch to **RAMJobStore**:

```csharp
// QuartzConfiguration.cs
services.AddQuartz(q =>
{
    // Temporarily use in-memory store
    q.UseInMemoryStore();
    
    // Job and trigger configuration remains the same
    // ...
});
```

### Long-term: Implement Persistent Store (Future)

When production scaling requires it:

1. **Run Quartz SQL Scripts**:
   - Download from: https://github.com/quartznet/quartznet/tree/main/database/tables
   - Execute `tables_sqlServer.sql` against the database

2. **Re-enable Persistent Configuration**:
   ```csharp
   q.UsePersistentStore(store =>
   {
       store.UseSqlServer(configuration.GetConnectionString("DefaultConnection")!);
       store.UseNewtonsoftJsonSerializer();
       store.PerformSchemaValidation = true;
   });
   ```

## Consequences

### In-Memory Store (Current)

| Aspect | Impact |
|--------|--------|
| **Startup** | ✅ Fast, no database dependency |
| **Restart** | ⚠️ Job execution history lost |
| **Clustering** | ❌ Cannot share jobs across instances |
| **Missed Jobs** | ⚠️ Jobs scheduled during downtime are lost |

### Persistent Store (Future)

| Aspect | Impact |
|--------|--------|
| **Startup** | Slower (DB connection required) |
| **Restart** | ✅ Jobs resume from saved state |
| **Clustering** | ✅ Multiple instances share job queue |
| **Missed Jobs** | ✅ Supports misfire handling |

## When to Migrate to Persistent Store

Consider migrating when:
- Running multiple API instances (load balancing)
- Critical jobs must not be missed during deployments
- Need detailed job execution history/audit
- Implementing job management UI

## Quartz Tables Required

For SQL Server persistent store, these tables are needed:

```
QRTZ_BLOB_TRIGGERS
QRTZ_CALENDARS  
QRTZ_CRON_TRIGGERS
QRTZ_FIRED_TRIGGERS
QRTZ_JOB_DETAILS
QRTZ_LOCKS
QRTZ_PAUSED_TRIGGER_GRPS
QRTZ_SCHEDULER_STATE
QRTZ_SIMPLE_TRIGGERS
QRTZ_SIMPROP_TRIGGERS
QRTZ_TRIGGERS
```

## Migration Script Location

Add Quartz table creation to EF migrations or run separately:

```sql
-- Option 1: Include in EF migration
-- Option 2: Run quartz SQL script during initial setup
-- Download: https://github.com/quartznet/quartznet/blob/main/database/tables/tables_sqlServer.sql
```

## Related Configuration

```csharp
// Current In-Memory Configuration
services.AddQuartz(q =>
{
    q.UseInMemoryStore();  // ← Current setting
    
    // Subscription Renewal - Daily 00:05 VN time
    var renewalJobKey = new JobKey("SubscriptionRenewalJob");
    q.AddJob<SubscriptionRenewalJob>(opts => opts.WithIdentity(renewalJobKey).StoreDurably());
    q.AddTrigger(opts => opts
        .ForJob(renewalJobKey)
        .WithIdentity("SubscriptionRenewalTrigger")
        .WithCronSchedule("0 5 0 * * ?", x => x
            .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))));
    
    // ... other jobs
});

services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
```

## Related ADRs
- [ADR 001: Database Choice](001-database-choice.md)

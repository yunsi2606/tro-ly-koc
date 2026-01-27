# ADR 003: Docker Environment Configuration

## Context
When starting the Docker environment, multiple services failed to connect properly:
1. **SQL Server:** `Login failed for user 'sa'` (Error 18456).
2. **Quartz.NET:** `Database schema validation failed`.
3. **API Container:** Could not resolve environment variables.

## Root Causes Identified

### 1. SQL Server Password Mismatch
- **Issue:** The `SA_PASSWORD` environment variable was not being passed correctly to the SQL Server container because the `.env` file was not being read properly by Docker Compose, or the password was set during initial volume creation and changing `.env` afterward had no effect.
- **Solution:** 
  - Added default values directly in `docker-compose.yml` using the `${VAR:-default}` syntax.
  - Deleted the SQL Server volume (`docker compose down -v`) to force a fresh initialization with the correct password.

### 2. Quartz.NET Schema Validation
- **Issue:** Quartz.NET with `UsePersistentStore` requires specific database tables (QRTZ_*) to exist. On first run, these tables were missing.
- **Solution:**
  - Added `PerformSchemaValidation = false` temporarily in `QuartzConfiguration.cs` to allow the app to start without crashing.
  - Created `docker/init-scripts/quartz_tables.sql` containing the official Quartz.NET SQL Server schema.
  - This script should be run manually after the database is created, or integrated into an init container/migration step.

### 3. Environment Variable Resolution in Docker
- **Issue:** Environment variables like `${SA_PASSWORD}` without defaults caused warnings and empty values when `.env` was missing or not loaded.
- **Solution:** Updated `docker-compose.yml` to use default values:
  ```yaml
  ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=TroLiKOC;User Id=sa;Password=${SA_PASSWORD:-YourStrong@Password123};TrustServerCertificate=True"
  ```

## Lessons Learned
1. **Always use default values** in `docker-compose.yml` for critical variables to prevent startup failures.
2. **Volume persistence** means changing passwords requires volume recreation.
3. **Schema validation** for job schedulers should be disabled during development or an init script should be run first.

## Current Configuration
- All services now use inline defaults for environment variables.
- `restart: unless-stopped` added to API container for resilience.
- Quartz schema validation disabled for development (re-enable in production after running init script).

## Status
**Accepted** - Resolved on 2026-01-25.

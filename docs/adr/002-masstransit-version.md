# ADR 002: MassTransit Version Selection

## Context
During API startup in Docker, the application crashed with the following error:
```
MassTransit.ConfigurationException: The bus configuration is invalid:
[Failure] License must be specified with SetLicense/SetLicenseLocation or by setting the MT_LICENSE/MT_LICENSE_PATH environment variables
```

## Problem
MassTransit version 9.0.0 (released Q1 2026) transitioned to a **commercial licensing model**. This means:
- A license key is now required for production use.
- Local development gets a temporary auto-generated license, but Docker containers are treated as non-local environments.

## Options Considered

### Option 1: Purchase MassTransit License
- **Pros:** Access to latest features, official support.
- **Cons:** Adds recurring cost to the project budget. Conflicts with the project's cost-efficiency goal.

### Option 2: Downgrade to MassTransit v8.3.0
- **Pros:** Last stable version under the open-source MIT license. No license fees. All required features (RabbitMQ transport, Topic exchanges) are available.
- **Cons:** No access to v9+ features. Potential future migration effort if we eventually need v9.

### Option 3: Replace MassTransit with raw RabbitMQ client
- **Pros:** No third-party library dependency.
- **Cons:** Significant development effort. Loss of MassTransit's abstractions (saga, retry, outbox).

## Decision
**We chose Option 2: Downgrade to MassTransit v8.3.0.**

### Reasoning
1. **Cost:** The project aims for cost efficiency. A commercial license is unnecessary for our scale.
2. **Feature Parity:** v8.3.0 provides all features we need (RabbitMQ transport, message publishing, consumers).
3. **Low Risk:** v8.3.0 is stable and well-documented.

## Implementation
Modified the following `.csproj` files:
- `TroLiKOC.API.csproj`: Changed `MassTransit.RabbitMQ` from `9.0.0` to `8.3.0`.
- `TroLiKOC.Modules.Jobs.csproj`: Changed `MassTransit` from `9.0.0` to `8.3.0`.

## Status
**Accepted** - Implemented on 2026-01-25.

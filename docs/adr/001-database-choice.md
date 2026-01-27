# ADR 001: Database Choice - SQL Server vs MongoDB

## Context
The project "Trợ Lý KOC" requires a database system to handle:
1.  **Identity & Access**: Users, Roles.
2.  **Financial Data**: Wallets, Transactions, Subscriptions (High consistency required).
3.  **Job Data**: Render jobs with varying input parameters based on AI models (Flexible schema required).
4.  **Tech Stack**: .NET 10 (Backend).

## Comparison

### 1. SQL Server (Relational)
*   **Pros:**
    *   **ACID Compliance:** Critical for the **Wallet** and **Payment** modules. Ensures money is never created or destroyed erroneously during race conditions.
    *   **Relational Integrity:** Strong enforcement of foreign keys (e.g., A Subscription must belong to a valid User and Tier).
    *   **Ecosystem:** First-class support in .NET via EF Core.
    *   **JSON Support:** Supports `JSON` columns, allowing `RenderJobs` to store flexible `InputPayload` without needing a NoSQL database.
*   **Cons:**
    *   Schema migrations required for structural changes.
    *   Horizontal scaling is harder/more expensive than NoSQL (though vertical scaling is sufficient for vast majority of SaaS).

### 2. MongoDB (NoSQL)
*   **Pros:**
    *   **Schema Flexibility:** Excellent for `RenderJobs` where each AI model (LivePortrait, IDM-VTON) has completely different parameters.
    *   **Write Performance:** Generally higher write throughput for logging and non-transactional data.
*   **Cons:**
    *   **Transactions:** Multi-document ACID transactions exist but are heavier and complex to implement correctly compared to SQL.
    *   **Data Integrity:** weaker enforcement of relationships (e.g., ensuring a wallet transaction links to a valid wallet requires app-level logic).
    *   **Reporting:** Complex queries (Joins) are harder to write and optimize.

## Analysis for "Trợ Lý KOC"

The most critical business risk for this SaaS is **financial inconsistency** (e.g., User top-up doesn't reflect, or double-spending).
*   **Wallet Module:** strictly requires Strong Consistency (ACID). SQL Server is the winner.
*   **Subscription Module:** Relational nature (User -> Tier). SQL Server is the winner.
*   **Jobs Module:** Requires flexibility.
    *   *Solution:* Use SQL Server with a `JSON` column (or `NVARCHAR(MAX)`) for the dynamic `InputPayload`. This gives the "best of both worlds" for this specific scale.

## Decision
**We will stick with SQL Server.**

### Reasoning:
1.  **Financial Safety:** The "No Free Tier" model means every job is paid for. We cannot risk financial data capability.
2.  **Complexity:** Introducing MongoDB just for the "Jobs" module adds infrastructure complexity (another container, another driver, distributed transaction issues).
3.  **Sufficiency:** SQL Server's JSON handling is sufficient for the job parameters.

### Status
Proposed

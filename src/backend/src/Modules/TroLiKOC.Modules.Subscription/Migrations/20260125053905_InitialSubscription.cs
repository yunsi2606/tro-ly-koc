using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TroLiKOC.Modules.Subscription.Migrations
{
    /// <inheritdoc />
    public partial class InitialSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "subscription");

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                schema: "subscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobsUsedThisMonth = table.Column<int>(type: "int", nullable: false),
                    LastRenewalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTiers",
                schema: "subscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxJobsPerMonth = table.Column<int>(type: "int", nullable: false),
                    MaxResolution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasWatermark = table.Column<bool>(type: "bit", nullable: false),
                    QueuePriority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportsLoRA = table.Column<bool>(type: "bit", nullable: false),
                    SupportsVoiceCloning = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTiers", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "subscription",
                table: "SubscriptionTiers",
                columns: new[] { "Id", "CreatedAt", "HasWatermark", "IsActive", "MaxJobsPerMonth", "MaxResolution", "MonthlyPrice", "Name", "QueuePriority", "SupportsLoRA", "SupportsVoiceCloning", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 25, 5, 39, 4, 513, DateTimeKind.Utc).AddTicks(9290), true, true, 50, "720p", 199000m, "Cơ Bản", "low", false, false, new DateTime(2026, 1, 25, 5, 39, 4, 513, DateTimeKind.Utc).AddTicks(9299) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 25, 5, 39, 4, 514, DateTimeKind.Utc).AddTicks(419), false, true, 200, "1080p", 499000m, "Sáng Tạo Nội Dung", "high", false, false, new DateTime(2026, 1, 25, 5, 39, 4, 514, DateTimeKind.Utc).AddTicks(420) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 25, 5, 39, 4, 514, DateTimeKind.Utc).AddTicks(421), false, true, -1, "4K", 1499000m, "Đại Lý", "realtime", true, true, new DateTime(2026, 1, 25, 5, 39, 4, 514, DateTimeKind.Utc).AddTicks(422) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                schema: "subscription",
                table: "Subscriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions",
                schema: "subscription");

            migrationBuilder.DropTable(
                name: "SubscriptionTiers",
                schema: "subscription");
        }
    }
}

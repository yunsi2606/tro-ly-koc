using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TroLiKOC.Modules.Subscription.Migrations
{
    /// <inheritdoc />
    public partial class FixSubscriptionSeedEx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "subscription",
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                schema: "subscription",
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                schema: "subscription",
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "subscription",
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 25, 5, 39, 4, 513, DateTimeKind.Utc).AddTicks(9290), new DateTime(2026, 1, 25, 5, 39, 4, 513, DateTimeKind.Utc).AddTicks(9299) });

            migrationBuilder.UpdateData(
                schema: "subscription",
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 25, 5, 39, 4, 514, DateTimeKind.Utc).AddTicks(419), new DateTime(2026, 1, 25, 5, 39, 4, 514, DateTimeKind.Utc).AddTicks(420) });

            migrationBuilder.UpdateData(
                schema: "subscription",
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 25, 5, 39, 4, 514, DateTimeKind.Utc).AddTicks(421), new DateTime(2026, 1, 25, 5, 39, 4, 514, DateTimeKind.Utc).AddTicks(422) });
        }
    }
}

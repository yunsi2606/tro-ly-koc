using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TroLiKOC.Modules.Jobs.Migrations
{
    /// <inheritdoc />
    public partial class InitialJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "jobs");

            migrationBuilder.CreateTable(
                name: "RenderJobs",
                schema: "jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutputKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessingTimeMs = table.Column<int>(type: "int", nullable: true),
                    QueuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenderJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenderJobs_Status",
                schema: "jobs",
                table: "RenderJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RenderJobs_UserId",
                schema: "jobs",
                table: "RenderJobs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenderJobs",
                schema: "jobs");
        }
    }
}

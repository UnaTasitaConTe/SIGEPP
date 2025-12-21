using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoryPpa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PpaHistoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PpaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    OldValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpaHistoryEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PpaHistoryEntries_PerformedAt",
                table: "PpaHistoryEntries",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PpaHistoryEntries_PerformedByUserId",
                table: "PpaHistoryEntries",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PpaHistoryEntries_PpaId",
                table: "PpaHistoryEntries",
                column: "PpaId");

            migrationBuilder.CreateIndex(
                name: "IX_PpaHistoryEntries_PpaId_ActionType",
                table: "PpaHistoryEntries",
                columns: new[] { "PpaId", "ActionType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PpaHistoryEntries");
        }
    }
}

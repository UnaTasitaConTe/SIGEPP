using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPpaStudentsAndContinuity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContinuationOfPpaId",
                table: "Ppas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContinuedByPpaId",
                table: "Ppas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PpaStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PpaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpaStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PpaStudents_Ppas_PpaId",
                        column: x => x.PpaId,
                        principalTable: "Ppas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ppas_ContinuationOfPpaId",
                table: "Ppas",
                column: "ContinuationOfPpaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ppas_ContinuedByPpaId",
                table: "Ppas",
                column: "ContinuedByPpaId");

            migrationBuilder.CreateIndex(
                name: "IX_PpaStudents_PpaId",
                table: "PpaStudents",
                column: "PpaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PpaStudents");

            migrationBuilder.DropIndex(
                name: "IX_Ppas_ContinuationOfPpaId",
                table: "Ppas");

            migrationBuilder.DropIndex(
                name: "IX_Ppas_ContinuedByPpaId",
                table: "Ppas");

            migrationBuilder.DropColumn(
                name: "ContinuationOfPpaId",
                table: "Ppas");

            migrationBuilder.DropColumn(
                name: "ContinuedByPpaId",
                table: "Ppas");
        }
    }
}

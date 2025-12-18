using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPpaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ppas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    GeneralObjective = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SpecificObjectives = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AcademicPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryTeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ppas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ppas_AcademicPeriods_AcademicPeriodId",
                        column: x => x.AcademicPeriodId,
                        principalTable: "AcademicPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ppas_Users_PrimaryTeacherId",
                        column: x => x.PrimaryTeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PpaAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PpaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FileKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpaAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PpaAttachments_Ppas_PpaId",
                        column: x => x.PpaId,
                        principalTable: "Ppas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PpaAttachments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PpaTeacherAssignments",
                columns: table => new
                {
                    PpaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherAssignmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpaTeacherAssignments", x => new { x.PpaId, x.TeacherAssignmentId });
                    table.ForeignKey(
                        name: "FK_PpaTeacherAssignments_Ppas_PpaId",
                        column: x => x.PpaId,
                        principalTable: "Ppas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PpaTeacherAssignments_TeacherAssignments_TeacherAssignmentId",
                        column: x => x.TeacherAssignmentId,
                        principalTable: "TeacherAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PpaAttachments_FileKey",
                table: "PpaAttachments",
                column: "FileKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PpaAttachments_IsDeleted",
                table: "PpaAttachments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PpaAttachments_Ppa_Type",
                table: "PpaAttachments",
                columns: new[] { "PpaId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PpaAttachments_PpaId",
                table: "PpaAttachments",
                column: "PpaId");

            migrationBuilder.CreateIndex(
                name: "IX_PpaAttachments_Type",
                table: "PpaAttachments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PpaAttachments_UploadedByUserId",
                table: "PpaAttachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ppas_AcademicPeriodId",
                table: "Ppas",
                column: "AcademicPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Ppas_PrimaryTeacherId",
                table: "Ppas",
                column: "PrimaryTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Ppas_Status",
                table: "Ppas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Ppas_Teacher_Period",
                table: "Ppas",
                columns: new[] { "PrimaryTeacherId", "AcademicPeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_PpaTeacherAssignments_PpaId",
                table: "PpaTeacherAssignments",
                column: "PpaId");

            migrationBuilder.CreateIndex(
                name: "IX_PpaTeacherAssignments_TeacherAssignmentId",
                table: "PpaTeacherAssignments",
                column: "TeacherAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PpaAttachments");

            migrationBuilder.DropTable(
                name: "PpaTeacherAssignments");

            migrationBuilder.DropTable(
                name: "Ppas");
        }
    }
}

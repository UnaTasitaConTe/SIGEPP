using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSecurityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "Description", "Module" },
                values: new object[,]
                {
                    { 1L, "view", "periods.view", "Ver períodos académicos", "periods" },
                    { 2L, "create", "periods.create", "Crear períodos académicos", "periods" },
                    { 3L, "update", "periods.update", "Actualizar períodos académicos", "periods" },
                    { 4L, "deactivate", "periods.deactivate", "Desactivar períodos académicos", "periods" },
                    { 5L, "view", "subjects.view", "Ver materias/asignaturas", "subjects" },
                    { 6L, "create", "subjects.create", "Crear materias/asignaturas", "subjects" },
                    { 7L, "update", "subjects.update", "Actualizar materias/asignaturas", "subjects" },
                    { 8L, "deactivate", "subjects.deactivate", "Desactivar materias/asignaturas", "subjects" },
                    { 9L, "manage", "teacherSubjects.manage", "Gestionar asignación docente-materia", "teacherSubjects" },
                    { 10L, "view_all", "ppa.view_all", "Ver todos los PPAs", "ppa" },
                    { 11L, "view_own", "ppa.view_own", "Ver sus propios PPAs", "ppa" },
                    { 12L, "create", "ppa.create", "Crear PPAs", "ppa" },
                    { 13L, "update", "ppa.update", "Actualizar PPAs", "ppa" },
                    { 14L, "change_status", "ppa.change_status", "Cambiar estado de PPAs", "ppa" },
                    { 15L, "upload_file", "ppa.upload_file", "Subir archivos a PPAs", "ppa" },
                    { 16L, "view_all", "resources.view_all", "Ver todos los recursos", "resources" },
                    { 17L, "view_own", "resources.view_own", "Ver sus propios recursos", "resources" },
                    { 18L, "create", "resources.create", "Crear recursos", "resources" },
                    { 19L, "update", "resources.update", "Actualizar recursos", "resources" },
                    { 20L, "delete", "resources.delete", "Eliminar recursos", "resources" },
                    { 21L, "view", "dashboard.view", "Ver dashboard", "dashboard" },
                    { 22L, "view_details", "dashboard.view_details", "Ver detalles del dashboard", "dashboard" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Code", "Description", "IsSystemRole", "Name" },
                values: new object[,]
                {
                    { 1L, "ADMIN", "Acceso completo a gestión académica (períodos, materias, asignaciones), supervisión de PPAs y dashboard completo.", true, "Administrador" },
                    { 2L, "DOCENTE", "Gestiona sus propios PPAs y recursos académicos, visualiza materias asignadas.", true, "Docente" },
                    { 3L, "CONSULTA_INTERNA", "Acceso de solo lectura para consulta y auditoría de PPAs, recursos y dashboard.", true, "Consulta Interna" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1L, 1L },
                    { 2L, 1L },
                    { 3L, 1L },
                    { 4L, 1L },
                    { 5L, 1L },
                    { 6L, 1L },
                    { 7L, 1L },
                    { 8L, 1L },
                    { 9L, 1L },
                    { 10L, 1L },
                    { 13L, 1L },
                    { 14L, 1L },
                    { 15L, 1L },
                    { 16L, 1L },
                    { 21L, 1L },
                    { 22L, 1L },
                    { 5L, 2L },
                    { 11L, 2L },
                    { 12L, 2L },
                    { 13L, 2L },
                    { 14L, 2L },
                    { 15L, 2L },
                    { 17L, 2L },
                    { 18L, 2L },
                    { 19L, 2L },
                    { 20L, 2L },
                    { 10L, 3L },
                    { 16L, 3L },
                    { 21L, 3L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Module_Action",
                table: "Permissions",
                columns: new[] { "Module", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsSystemRole",
                table: "Roles",
                column: "IsSystemRole");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}

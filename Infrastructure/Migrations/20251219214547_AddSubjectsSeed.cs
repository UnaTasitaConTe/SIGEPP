using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("00ccb6b6-8d31-e7be-987f-c608d6317abf"), "124-I-02", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INGLES A1:2", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("04a14d1a-110c-da5b-9410-2a5a008b677f"), "124-0737", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ELECTIVA II", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("06349c30-3880-a261-ea8b-cc01cff8f647"), "124-0720", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ALGEBRA LINEAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("1616574f-71a3-f049-e656-d7ea3414c109"), "124-0729", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "DESARROLLO BASICO APLICACIONES EN RED", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("16b71051-0257-a6bb-6b12-3cfc395feb59"), "124-1076", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PRACTICA EMPRESARIAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("17ffa3fd-70b8-387f-91bd-00e39fc72f8c"), "124-0613", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "CATEDRA FESC, CULTURA Y VALORES", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("1c6d2287-2bac-434d-60d0-356e205585c9"), "124-0728", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PROGRAMACION ORIENTADA A OBJETOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("2063ad81-2b78-6fdc-55e0-5fd70ecc6c8a"), "124-0733", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "MODELOS Y DOCUMENTACION DEL SOFTWARE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("254c6e53-37df-de3f-0258-719148e91956"), "124-0725", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "SISTEMAS OPERATIVOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("2a836363-74ba-f2a1-b9b2-b68abd4e59c9"), "124-3024", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "EMPRENDIMIENTO", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("2cfcc659-3a17-7fb6-cb2f-bb374705bbc9"), "124-0779", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ARQUITECTURA CLIENTE SERVIDOR - Propedeu", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("36fbad24-a3dc-11aa-9f56-cb703a2c50ba"), "124-0734", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ARQUITECTURA Y DISEÑO DEL SOFTWARE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("3c24ed93-c383-87c6-f420-4f52ea8d1692"), "125-0744", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "METODOS NUMERICOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("407819d6-02a4-1d37-794e-f7b04152e545"), "125-0745", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INVESTIGACION DE OPERACIONES", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("45c27cc6-a6e1-981d-86cf-2f8fee5029e0"), "124-0726", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "LOGICA Y FUNDAMENTOS DE PROGRAMACION", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("4d0d252e-febb-8bbc-b705-a5b90d4a89c7"), "124-0741", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "BASE DE DATOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50410699-e483-2ee3-a7fb-b4ca1395c3d8"), "124-I-03", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INGLES A2:1", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50d6753b-4431-fd06-8b88-f7f3dd2f2c7d"), "124-0743", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ESTADISTICA Y PROBABILIDAD", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("589b6945-2dae-1b3e-3a2b-ffd185c16b27"), "125-0703", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "FORMULACION Y EVALUACION DE PROYECTOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("5997ccd5-4b36-5cf5-b7a5-91170d49e558"), "124-0715", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ELECTRONICA DIGITAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("5e2fd604-e12f-e129-e8a9-cbf4b5ac4024"), "124-0732", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "METODOLOGIAS DESARROLLO DEL SOFTWARE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("6590fd8c-f623-1b33-7dba-54cc32b7eec2"), "124-0718", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "CALCULO DIFERENCIAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("6ca9081a-19fc-dfbb-321b-9a9021b8350d"), "124-0071", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PLAN DE NEGOCIOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("703349a9-3411-8e41-b9c2-58d3107c7df2"), "125-0754", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "CALIDAD Y SEGURIDAD DEL SOFTWARE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("76094584-b2cf-b2e8-2381-0cd74bcecac0"), "125-0750", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "DISEÑO MOVIL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("767457a1-8dce-0706-b493-bfaf75ab27a8"), "124-8047", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INVESTIGACION DE MERCADOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("7cca5530-eb02-4dd6-ce73-6aa0cc4f8d63"), "125-0749", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INTELIGENCIA ARTIFICIAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8b53cdc8-7f9f-e465-6376-8423dca61314"), "124-0716", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "FISICA MECANICA", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8d890004-277c-2471-8e18-21310b663799"), "124-0742", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ANALISIS DE RQUERIMIENTOS DEL SOFTWARE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("977dc821-4277-fa2c-b181-087f4698d03e"), "124-0727", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PROGRAMACION ESTRUCTURADA", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("9cdd082c-c6ca-914f-bc28-d313825b11b9"), "124-0717", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "FISICA ELECTROMAGNETICA", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a08ae93d-f6c3-90f1-f26c-8a91bdeabf21"), "125-0751", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "DISEÑO FUNCIONAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a294b1a7-cb3e-9c38-fc68-94d15e7e2389"), "124-0723", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INSTALACION Y CONFIGURACION DE REDES", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aa5caf57-85d2-dad8-e34e-16faedea6913"), "124-0082", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "CONTABILIDAD Y COSTOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ad9afd67-2927-4c6e-920d-777dfc341ab7"), "125-1077", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PRACTICA EMPRESARIAL EMPRESARIAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bd6c7108-833e-d420-15bb-7a80fe9cc141"), "125-0746", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "TEORIA DE DECISIONES", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("beba8df3-fc30-5a4f-fc91-b085b981f806"), "124-0730", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "DESARROLLO AVANZADO APLICACIONES EN RED", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("cc0f1826-8069-381b-8aa6-fe9b0aa411e4"), "124-0722", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ENSAMBLE Y MANTENIMIENTO DE COMPUTADORES", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("cd75b1e7-1fb6-db58-b149-c3f127531fc8"), "124-0069", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PRODUCCION Y ANALISIS DE TEXTOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d2a2acce-7e0c-2f72-c678-85ef7da31698"), "124-0731", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INTRODUC A LA INGENIERIA DEL SOFTWARE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d6861d03-0d01-8dee-7c04-b7398f4e0559"), "124-0724", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "OFIMATICA, REDES SOCIALES Y COLABORATIVA", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d9b42b42-8a22-1e32-1b8c-34d2f041af6f"), "124-0719", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "CALCULO INTEGRAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("dac58feb-ab18-748c-25ff-58997afe01b1"), "125-0753", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "GESTION DE PROYECTOS DE SOFTWARE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("db1c982a-6d90-e25a-c1a1-320b3331bc17"), "125-0747", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ACCESIBILIDAD Y USABILIDAD", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("dd97c328-87a0-8922-3fbf-41e623543786"), "124-7022", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "HABILIDADES COMUNICATIVAS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ddd79bb1-a388-6cca-fdf3-6386da149c6c"), "124-8033", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "METODOLOGIA DE LA INVESTIGACION", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("e480a49b-24f0-9507-6f4b-b98ec85e18f0"), "124-0820", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ECUACIONES DIFERENCIALES", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ec385ffc-e50b-598e-eb32-e0a2d025642e"), "125-0748", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "TECNOLOGIAS EMERGENTES", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("f2894310-a209-b756-a547-b3268bbfb67b"), "124-0736", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "ELECTIVA I", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("f6778165-dadc-4b14-4694-2bfcd7af0e3a"), "124-0735", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "CODIFICACION Y PRUEBAS DEL SOFTWARE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("f77b466c-1dd8-2ab6-b35f-7277e9e07f10"), "124-I-01", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "INGLES A1:1", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("fab0d1ca-e941-16c1-5f64-b236f5a55440"), "125-0752", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "DISEÑO MULTIMEDIA Y VIDEOJUEGOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("fb61df01-dd77-c74e-91d9-609548c0ca69"), "124-0721", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "CALCULO MULTIVARIADO", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("00ccb6b6-8d31-e7be-987f-c608d6317abf"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("04a14d1a-110c-da5b-9410-2a5a008b677f"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("06349c30-3880-a261-ea8b-cc01cff8f647"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("1616574f-71a3-f049-e656-d7ea3414c109"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("16b71051-0257-a6bb-6b12-3cfc395feb59"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("17ffa3fd-70b8-387f-91bd-00e39fc72f8c"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("1c6d2287-2bac-434d-60d0-356e205585c9"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("2063ad81-2b78-6fdc-55e0-5fd70ecc6c8a"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("254c6e53-37df-de3f-0258-719148e91956"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("2a836363-74ba-f2a1-b9b2-b68abd4e59c9"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("2cfcc659-3a17-7fb6-cb2f-bb374705bbc9"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("36fbad24-a3dc-11aa-9f56-cb703a2c50ba"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("3c24ed93-c383-87c6-f420-4f52ea8d1692"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("407819d6-02a4-1d37-794e-f7b04152e545"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("45c27cc6-a6e1-981d-86cf-2f8fee5029e0"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("4d0d252e-febb-8bbc-b705-a5b90d4a89c7"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("50410699-e483-2ee3-a7fb-b4ca1395c3d8"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("50d6753b-4431-fd06-8b88-f7f3dd2f2c7d"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("589b6945-2dae-1b3e-3a2b-ffd185c16b27"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("5997ccd5-4b36-5cf5-b7a5-91170d49e558"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("5e2fd604-e12f-e129-e8a9-cbf4b5ac4024"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("6590fd8c-f623-1b33-7dba-54cc32b7eec2"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("6ca9081a-19fc-dfbb-321b-9a9021b8350d"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("703349a9-3411-8e41-b9c2-58d3107c7df2"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("76094584-b2cf-b2e8-2381-0cd74bcecac0"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("767457a1-8dce-0706-b493-bfaf75ab27a8"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("7cca5530-eb02-4dd6-ce73-6aa0cc4f8d63"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("8b53cdc8-7f9f-e465-6376-8423dca61314"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("8d890004-277c-2471-8e18-21310b663799"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("977dc821-4277-fa2c-b181-087f4698d03e"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("9cdd082c-c6ca-914f-bc28-d313825b11b9"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("a08ae93d-f6c3-90f1-f26c-8a91bdeabf21"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("a294b1a7-cb3e-9c38-fc68-94d15e7e2389"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("aa5caf57-85d2-dad8-e34e-16faedea6913"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("ad9afd67-2927-4c6e-920d-777dfc341ab7"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("bd6c7108-833e-d420-15bb-7a80fe9cc141"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("beba8df3-fc30-5a4f-fc91-b085b981f806"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("cc0f1826-8069-381b-8aa6-fe9b0aa411e4"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("cd75b1e7-1fb6-db58-b149-c3f127531fc8"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("d2a2acce-7e0c-2f72-c678-85ef7da31698"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("d6861d03-0d01-8dee-7c04-b7398f4e0559"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("d9b42b42-8a22-1e32-1b8c-34d2f041af6f"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("dac58feb-ab18-748c-25ff-58997afe01b1"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("db1c982a-6d90-e25a-c1a1-320b3331bc17"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("dd97c328-87a0-8922-3fbf-41e623543786"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("ddd79bb1-a388-6cca-fdf3-6386da149c6c"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("e480a49b-24f0-9507-6f4b-b98ec85e18f0"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("ec385ffc-e50b-598e-eb32-e0a2d025642e"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("f2894310-a209-b756-a547-b3268bbfb67b"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("f6778165-dadc-4b14-4694-2bfcd7af0e3a"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("f77b466c-1dd8-2ab6-b35f-7277e9e07f10"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("fab0d1ca-e941-16c1-5f64-b236f5a55440"));

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: new Guid("fb61df01-dd77-c74e-91d9-609548c0ca69"));
        }
    }
}

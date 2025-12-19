using Infrastructure.Persistence.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Persistence.Seeds
{
    public static class SubjectSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var now = DateTime.SpecifyKind(new DateTime(2025, 01, 01, 0, 0, 0), DateTimeKind.Utc);

            var subjects = new List<SubjectEntity>
        {
            // =========================
            // 125 - Electivas / Finales
            // =========================
            New("125-0703", "FORMULACION Y EVALUACION DE PROYECTOS", now),
            New("125-0744", "METODOS NUMERICOS", now),
            New("125-0747", "ACCESIBILIDAD Y USABILIDAD", now),
            New("125-0750", "DISEÑO MOVIL", now),

            New("125-0745", "INVESTIGACION DE OPERACIONES", now),
            New("125-0748", "TECNOLOGIAS EMERGENTES", now),
            New("125-0751", "DISEÑO FUNCIONAL", now),
            New("125-0753", "GESTION DE PROYECTOS DE SOFTWARE", now),

            New("125-0746", "TEORIA DE DECISIONES", now),
            New("125-0749", "INTELIGENCIA ARTIFICIAL", now),
            New("125-0752", "DISEÑO MULTIMEDIA Y VIDEOJUEGOS", now),
            New("125-0754", "CALIDAD Y SEGURIDAD DEL SOFTWARE", now),

            New("125-1077", "PRACTICA EMPRESARIAL EMPRESARIAL", now),

            // =========================
            // 124 - Materias del plan
            // =========================
            New("124-0613", "CATEDRA FESC, CULTURA Y VALORES", now),
            New("124-0715", "ELECTRONICA DIGITAL", now),
            New("124-0722", "ENSAMBLE Y MANTENIMIENTO DE COMPUTADORES", now),
            New("124-0726", "LOGICA Y FUNDAMENTOS DE PROGRAMACION", now),
            New("124-0731", "INTRODUC A LA INGENIERIA DEL SOFTWARE", now),
            New("124-7022", "HABILIDADES COMUNICATIVAS", now),

            New("124-0716", "FISICA MECANICA", now),
            New("124-0718", "CALCULO DIFERENCIAL", now),
            New("124-0723", "INSTALACION Y CONFIGURACION DE REDES", now),
            New("124-0727", "PROGRAMACION ESTRUCTURADA", now),
            New("124-0732", "METODOLOGIAS DESARROLLO DEL SOFTWARE", now),
            New("124-3024", "EMPRENDIMIENTO", now),

            New("124-0069", "PRODUCCION Y ANALISIS DE TEXTOS", now),
            New("124-0717", "FISICA ELECTROMAGNETICA", now),
            New("124-0719", "CALCULO INTEGRAL", now),
            New("124-0724", "OFIMATICA, REDES SOCIALES Y COLABORATIVA", now),
            New("124-0733", "MODELOS Y DOCUMENTACION DEL SOFTWARE", now),
            New("124-0741", "BASE DE DATOS", now),

            New("124-8047", "INVESTIGACION DE MERCADOS", now),
            New("124-I-01", "INGLES A1:1", now),

            New("124-0082", "CONTABILIDAD Y COSTOS", now),
            New("124-0720", "ALGEBRA LINEAL", now),
            New("124-0725", "SISTEMAS OPERATIVOS", now),
            New("124-0728", "PROGRAMACION ORIENTADA A OBJETOS", now),
            New("124-0736", "ELECTIVA I", now),
            New("124-0742", "ANALISIS DE RQUERIMIENTOS DEL SOFTWARE", now),

            New("124-I-02", "INGLES A1:2", now),

            New("124-0071", "PLAN DE NEGOCIOS", now),
            New("124-0721", "CALCULO MULTIVARIADO", now),
            New("124-0729", "DESARROLLO BASICO APLICACIONES EN RED", now),
            New("124-0734", "ARQUITECTURA Y DISEÑO DEL SOFTWARE", now),
            New("124-0743", "ESTADISTICA Y PROBABILIDAD", now),
            New("124-0779", "ARQUITECTURA CLIENTE SERVIDOR - Propedeu", now),

            New("124-8033", "METODOLOGIA DE LA INVESTIGACION", now),
            New("124-I-03", "INGLES A2:1", now),

            New("124-0730", "DESARROLLO AVANZADO APLICACIONES EN RED", now),
            New("124-0735", "CODIFICACION Y PRUEBAS DEL SOFTWARE", now),
            New("124-0737", "ELECTIVA II", now),
            New("124-0820", "ECUACIONES DIFERENCIALES", now),

            New("124-1076", "PRACTICA EMPRESARIAL", now),
        };

            // Quitar duplicados por Code (por si en el futuro pegas listas repetidas)
            subjects = subjects
                .GroupBy(s => s.Code, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            modelBuilder.Entity<SubjectEntity>().HasData(subjects);
        }

        private static SubjectEntity New(string code, string name, DateTime nowUtc) => new()
        {
            Id = DeterministicGuidFromCode(code),
            Code = code.Trim(),
            Name = name.Trim(),
            Description = null,
            IsActive = true,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        /// <summary>
        /// Genera un Guid estable a partir del código.
        /// Útil para HasData (IDs fijos) y para referenciar luego en otros seeds.
        /// </summary>
        private static Guid DeterministicGuidFromCode(string code)
        {
            // MD5 => 16 bytes => Guid estable
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(code.Trim().ToUpperInvariant()));
            return new Guid(bytes);
        }
    }
}

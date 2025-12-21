using Domain.Ppa;

namespace Domain.Dictionaries
{
    public static class PpaAttachmentTypeTranslations
    {
        public static readonly Dictionary<PpaAttachmentType, string> Spanish = new()
        {
            { PpaAttachmentType.PpaDocument, "Documento PPA" },
            { PpaAttachmentType.TeacherAuthorization, "Autorización Docente" },
            { PpaAttachmentType.StudentAuthorization, "Autorización Estudiantil" },
            { PpaAttachmentType.SourceCode, "Código Fuente" },
            { PpaAttachmentType.Presentation, "Presentación" },
            { PpaAttachmentType.Instrument, "Instrumento de Investigación" },
            { PpaAttachmentType.Evidence, "Evidencia" },
            { PpaAttachmentType.Other, "Otro" }
        };

        public static readonly Dictionary<PpaAttachmentType, string> English = new()
        {
            { PpaAttachmentType.PpaDocument, "PPA Document" },
            { PpaAttachmentType.TeacherAuthorization, "Teacher Authorization" },
            { PpaAttachmentType.StudentAuthorization, "Student Authorization" },
            { PpaAttachmentType.SourceCode, "Source Code" },
            { PpaAttachmentType.Presentation, "Presentation" },
            { PpaAttachmentType.Instrument, "Research Instrument" },
            { PpaAttachmentType.Evidence, "Evidence" },
            { PpaAttachmentType.Other, "Other" }
        };
    }
}

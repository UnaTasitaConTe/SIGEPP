namespace Domain.Ppa;

/// <summary>
/// Tipos de anexos que pueden estar asociados a un PPA.
/// </summary>
public enum PpaAttachmentType
{
    /// <summary>
    /// Documento formal del PPA.
    /// Este es el archivo principal del proyecto (típicamente PDF) que contiene el trabajo académico completo.
    /// Un PPA debe tener al menos un anexo de este tipo para considerarse Completed.
    /// </summary>
    PpaDocument = 0,

    /// <summary>
    /// Autorización firmada por el docente responsable del PPA.
    /// </summary>
    TeacherAuthorization = 1,

    /// <summary>
    /// Autorización firmada por el estudiante o grupo de estudiantes involucrados.
    /// </summary>
    StudentAuthorization = 2,

    /// <summary>
    /// Código fuente del proyecto (en caso de proyectos de desarrollo de software).
    /// Puede ser un archivo ZIP, enlace a repositorio, etc.
    /// </summary>
    SourceCode = 3,

    /// <summary>
    /// Presentación del proyecto (diapositivas, video, etc.).
    /// </summary>
    Presentation = 4,

    /// <summary>
    /// Instrumentos de investigación (encuestas, cuestionarios, guías de entrevista, etc.).
    /// </summary>
    Instrument = 5,

    /// <summary>
    /// Evidencias del desarrollo del proyecto (fotografías, videos, registros, bitácoras, etc.).
    /// </summary>
    Evidence = 6,

    /// <summary>
    /// Otro tipo de anexo no clasificado en las categorías anteriores.
    /// </summary>
    Other = 7
}

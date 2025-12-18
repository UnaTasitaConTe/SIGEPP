namespace Infrastructure.Persistence.Entities.Ppa;

/// <summary>
/// Entity de EF Core que representa un anexo de PPA en la base de datos.
/// Mapea a la tabla PpaAttachments.
/// </summary>
public class PpaAttachmentEntity
{
    /// <summary>
    /// Identificador único del anexo.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del PPA al que pertenece este anexo.
    /// </summary>
    public Guid PpaId { get; set; }

    /// <summary>
    /// Tipo de anexo (mapeado al enum PpaAttachmentType: 0=PpaDocument, 1=TeacherAuthorization, etc.).
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Nombre amigable del anexo para mostrar al usuario.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Clave o ruta lógica del archivo en el sistema de almacenamiento.
    /// </summary>
    public string FileKey { get; set; } = string.Empty;

    /// <summary>
    /// Tipo MIME del archivo (opcional).
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Identificador del usuario que subió el anexo.
    /// </summary>
    public Guid UploadedByUserId { get; set; }

    /// <summary>
    /// Fecha y hora de carga del anexo.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Indica si el anexo ha sido marcado como eliminado (soft delete).
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Fecha y hora en que el anexo fue eliminado.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // Navegaciones
    /// <summary>
    /// Navegación hacia el PPA al que pertenece.
    /// </summary>
    public PpaEntity? Ppa { get; set; }

    /// <summary>
    /// Navegación hacia el usuario que subió el anexo.
    /// </summary>
    public UserEntity? UploadedBy { get; set; }
}

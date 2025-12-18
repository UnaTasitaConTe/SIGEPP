namespace Domain.Ppa.Entities;

/// <summary>
/// Representa un anexo (archivo físico) asociado a un PPA.
///
/// Los anexos incluyen:
/// - El documento formal del PPA (tipo <see cref="PpaAttachmentType.PpaDocument"/>)
/// - Autorizaciones, código fuente, presentaciones, instrumentos, evidencias, etc.
///
/// REGLA IMPORTANTE:
/// El PPA también se maneja como un anexo. El documento formal del PPA (típicamente PDF)
/// se representa como un anexo de tipo PpaDocument.
///
/// Reglas de negocio (implementadas en Application):
/// - Un PPA debe tener al menos un anexo de tipo PpaDocument para considerarse Completed.
/// - No se puede eliminar el último PpaDocument si el PPA está Completed.
/// </summary>
public sealed class PpaAttachment
{
    /// <summary>
    /// Identificador único del anexo.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identificador del PPA al que pertenece este anexo.
    /// </summary>
    public Guid PpaId { get; private set; }

    /// <summary>
    /// Tipo de anexo (documento PPA, autorización, evidencia, etc.).
    /// </summary>
    public PpaAttachmentType Type { get; private set; }

    /// <summary>
    /// Nombre amigable del anexo para mostrar al usuario.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Clave o ruta lógica del archivo en el sistema de almacenamiento.
    /// Esta clave es independiente del proveedor de almacenamiento (S3, Azure Blob, sistema de archivos, etc.).
    /// </summary>
    public string FileKey { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo MIME del archivo (opcional).
    /// Ejemplo: "application/pdf", "image/png", "application/zip".
    /// </summary>
    public string? ContentType { get; private set; }

    /// <summary>
    /// Identificador del usuario que subió el anexo.
    /// </summary>
    public Guid UploadedByUserId { get; private set; }

    /// <summary>
    /// Fecha y hora de carga del anexo.
    /// </summary>
    public DateTime UploadedAt { get; private set; }

    /// <summary>
    /// Indica si el anexo ha sido marcado como eliminado (soft delete).
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Fecha y hora en que el anexo fue eliminado.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    // Constructor privado para control total sobre la creación
    private PpaAttachment(
        Guid id,
        Guid ppaId,
        PpaAttachmentType type,
        string name,
        string fileKey,
        Guid uploadedByUserId,
        DateTime uploadedAt)
    {
        Id = id;
        PpaId = ppaId;
        Type = type;
        Name = name;
        FileKey = fileKey;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = uploadedAt;
        IsDeleted = false;
    }

    /// <summary>
    /// Crea un nuevo anexo de PPA.
    /// </summary>
    /// <param name="ppaId">ID del PPA al que pertenece.</param>
    /// <param name="type">Tipo de anexo.</param>
    /// <param name="name">Nombre amigable del anexo.</param>
    /// <param name="fileKey">Clave del archivo en el almacenamiento.</param>
    /// <param name="uploadedByUserId">ID del usuario que sube el archivo.</param>
    /// <param name="contentType">Tipo MIME (opcional).</param>
    /// <returns>Nueva instancia de PpaAttachment.</returns>
    /// <exception cref="ArgumentException">Si algún parámetro obligatorio es inválido.</exception>
    public static PpaAttachment Create(
        Guid ppaId,
        PpaAttachmentType type,
        string name,
        string fileKey,
        Guid uploadedByUserId,
        string? contentType = null)
    {
        if (ppaId == Guid.Empty)
            throw new ArgumentException("El ID del PPA no puede estar vacío.", nameof(ppaId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del anexo es obligatorio.", nameof(name));

        if (string.IsNullOrWhiteSpace(fileKey))
            throw new ArgumentException("La clave del archivo es obligatoria.", nameof(fileKey));

        if (uploadedByUserId == Guid.Empty)
            throw new ArgumentException("El ID del usuario que sube el archivo no puede estar vacío.", nameof(uploadedByUserId));

        var attachment = new PpaAttachment(
            Guid.NewGuid(),
            ppaId,
            type,
            name.Trim(),
            fileKey.Trim(),
            uploadedByUserId,
            DateTime.UtcNow)
        {
            ContentType = contentType?.Trim()
        };

        return attachment;
    }

    /// <summary>
    /// Factory para reconstruir un anexo existente desde persistencia.
    /// </summary>
    public static PpaAttachment CreateWithId(
        Guid id,
        Guid ppaId,
        PpaAttachmentType type,
        string name,
        string fileKey,
        Guid uploadedByUserId,
        DateTime uploadedAt,
        string? contentType = null,
        bool isDeleted = false,
        DateTime? deletedAt = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del anexo no puede estar vacío.", nameof(id));

        if (ppaId == Guid.Empty)
            throw new ArgumentException("El ID del PPA no puede estar vacío.", nameof(ppaId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del anexo es obligatorio.", nameof(name));

        if (string.IsNullOrWhiteSpace(fileKey))
            throw new ArgumentException("La clave del archivo es obligatoria.", nameof(fileKey));

        if (uploadedByUserId == Guid.Empty)
            throw new ArgumentException("El ID del usuario que sube el archivo no puede estar vacío.", nameof(uploadedByUserId));

        var attachment = new PpaAttachment(
            id,
            ppaId,
            type,
            name.Trim(),
            fileKey.Trim(),
            uploadedByUserId,
            uploadedAt)
        {
            ContentType = contentType?.Trim(),
            IsDeleted = isDeleted,
            DeletedAt = deletedAt
        };

        return attachment;
    }

    /// <summary>
    /// Marca el anexo como eliminado (soft delete).
    /// </summary>
    /// <param name="deletedAt">Fecha y hora de eliminación.</param>
    /// <exception cref="InvalidOperationException">Si el anexo ya está eliminado.</exception>
    public void MarkAsDeleted(DateTime deletedAt)
    {
        if (IsDeleted)
            throw new InvalidOperationException("El anexo ya está marcado como eliminado.");

        IsDeleted = true;
        DeletedAt = deletedAt;
    }

    /// <summary>
    /// Restaura un anexo previamente marcado como eliminado.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si el anexo no está eliminado.</exception>
    public void Restore()
    {
        if (!IsDeleted)
            throw new InvalidOperationException("El anexo no está marcado como eliminado.");

        IsDeleted = false;
        DeletedAt = null;
    }
}

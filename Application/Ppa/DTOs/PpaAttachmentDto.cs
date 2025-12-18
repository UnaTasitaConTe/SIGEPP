using Domain.Ppa;

namespace Application.Ppa.DTOs;

/// <summary>
/// DTO de anexo de PPA para respuestas de la API.
/// </summary>
public sealed record PpaAttachmentDto
{
    /// <summary>
    /// ID del anexo.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// ID del PPA al que pertenece este anexo.
    /// </summary>
    public required Guid PpaId { get; init; }

    /// <summary>
    /// Tipo de anexo (documento PPA, autorización, evidencia, etc.).
    /// </summary>
    public required PpaAttachmentType Type { get; init; }

    /// <summary>
    /// Nombre amigable del anexo para mostrar al usuario.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Clave o ruta lógica del archivo en el sistema de almacenamiento.
    /// </summary>
    public required string FileKey { get; init; }

    /// <summary>
    /// Tipo MIME del archivo.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// ID del usuario que subió el anexo.
    /// </summary>
    public required Guid UploadedByUserId { get; init; }

    /// <summary>
    /// Fecha y hora de carga del anexo.
    /// </summary>
    public required DateTime UploadedAt { get; init; }

    /// <summary>
    /// Indica si el anexo ha sido marcado como eliminado.
    /// </summary>
    public required bool IsDeleted { get; init; }
}

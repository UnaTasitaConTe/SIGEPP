namespace Application.Academics.DTOs;

/// <summary>
/// DTO básico de asignatura para respuestas de la API.
/// Contiene información esencial sin exponer el modelo de dominio.
/// </summary>
public sealed record SubjectDto
{
    /// <summary>
    /// ID de la asignatura.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Código único de la asignatura (ej: "ISW-101", "MAT-201").
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Nombre de la asignatura (ej: "Ingeniería de Software I").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Descripción opcional de la asignatura.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Indica si la asignatura está activa.
    /// </summary>
    public required bool IsActive { get; init; }
}

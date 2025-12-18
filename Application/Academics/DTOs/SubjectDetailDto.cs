namespace Application.Academics.DTOs;

/// <summary>
/// DTO detallado de asignatura para respuestas de la API.
/// Incluye información adicional útil para vistas de detalle.
/// Por ahora es similar al DTO básico, pero está preparado para agregar más campos en el futuro
/// (ej: cantidad de docentes asignados, períodos donde se dicta, etc.).
/// </summary>
public sealed record SubjectDetailDto
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

    /// <summary>
    /// Fecha de creación de la asignatura.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha de última actualización de la asignatura.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}

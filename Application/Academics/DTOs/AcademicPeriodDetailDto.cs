namespace Application.Academics.DTOs;

/// <summary>
/// DTO detallado de período académico para respuestas de la API.
/// Incluye información adicional útil para vistas de detalle.
/// Por ahora es igual al DTO básico, pero está preparado para agregar más campos en el futuro
/// (ej: cantidad de asignaciones activas, docentes asignados, etc.).
/// </summary>
public sealed record AcademicPeriodDetailDto
{
    /// <summary>
    /// ID del período académico.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Código único del período académico (ej: "2024-1", "2024-2").
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Nombre descriptivo del período académico (ej: "Periodo 2024-1").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Fecha de inicio del período académico (opcional).
    /// </summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>
    /// Fecha de fin del período académico (opcional).
    /// </summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>
    /// Indica si el período académico está activo.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Fecha de creación del período académico.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha de última actualización del período académico.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Indica si el período académico está vigente actualmente.
    /// Se calcula comparando las fechas con la fecha actual.
    /// </summary>
    public bool IsCurrent { get; init; }
}

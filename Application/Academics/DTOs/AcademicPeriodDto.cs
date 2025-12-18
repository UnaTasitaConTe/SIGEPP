namespace Application.Academics.DTOs;

/// <summary>
/// DTO básico de período académico para respuestas de la API.
/// Contiene información esencial sin exponer el modelo de dominio.
/// </summary>
public sealed record AcademicPeriodDto
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
}

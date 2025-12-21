using Domain.Ppa;

namespace Application.Ppa.DTOs;

/// <summary>
/// DTO resumido de un PPA para listados y vistas generales.
/// </summary>
public class PpaSummaryDto
{
    /// <summary>
    /// ID del PPA.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Título del PPA.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual del PPA.
    /// </summary>
    public PpaStatus Status { get; set; }

    /// <summary>
    /// ID del período académico.
    /// </summary>
    public Guid AcademicPeriodId { get; set; }

    /// <summary>
    /// Código del período académico (ej: "2024-1").
    /// </summary>
    public string? AcademicPeriodCode { get; set; }

    /// <summary>
    /// ID del docente responsable.
    /// </summary>
    public Guid ResponsibleTeacherId { get; set; }

    /// <summary>
    /// Nombre del docente responsable.
    /// </summary>
    public string? ResponsibleTeacherName { get; set; }

    /// <summary>
    /// Cantidad de asignaciones docente-asignatura asociadas.
    /// </summary>
    public int AssignmentsCount { get; set; }

    /// <summary>
    /// Cantidad de estudiantes asociados.
    /// </summary>
    public int StudentsCount { get; set; }

    /// <summary>
    /// Indica si el PPA es continuación de otro.
    /// </summary>
    public bool IsContinuation { get; set; }

    /// <summary>
    /// Indica si el PPA ha sido continuado en otro periodo.
    /// </summary>
    public bool HasContinuation { get; set; }

    /// <summary>
    /// Fecha de creación del PPA.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última actualización.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

using Domain.Ppa;

namespace Application.Ppa.DTOs;

/// <summary>
/// DTO detallado de PPA con información completa incluyendo asignaciones docente-asignatura.
/// </summary>
public sealed record PpaDetailDto
{
    /// <summary>
    /// ID del PPA.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Título del PPA (identificador principal del proyecto).
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Descripción general del PPA.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Objetivo general del PPA.
    /// </summary>
    public string? GeneralObjective { get; init; }

    /// <summary>
    /// Objetivos específicos del PPA.
    /// </summary>
    public string? SpecificObjectives { get; init; }

    /// <summary>
    /// Estado actual del PPA en su ciclo de vida.
    /// </summary>
    public required PpaStatus Status { get; init; }

    /// <summary>
    /// ID del período académico en el que se desarrolla el PPA.
    /// </summary>
    public required Guid AcademicPeriodId { get; init; }

    /// <summary>
    /// Código del período académico (información agregada).
    /// </summary>
    public string? AcademicPeriodCode { get; init; }

    /// <summary>
    /// ID del docente principal responsable del PPA.
    /// </summary>
    public required Guid PrimaryTeacherId { get; init; }

    /// <summary>
    /// Nombre del docente principal (información agregada).
    /// </summary>
    public string? PrimaryTeacherName { get; init; }

    /// <summary>
    /// IDs de las asignaciones docente-asignatura relacionadas con este PPA.
    /// </summary>
    public required IReadOnlyCollection<Guid> TeacherAssignmentIds { get; init; }

    /// <summary>
    /// Detalles de las asignaciones docente-asignatura relacionadas con este PPA,
    /// incluyendo nombres de asignaturas y docentes.
    /// </summary>
    public IReadOnlyCollection<PpaTeacherAssignmentDetailDto>? AssignmentDetails { get; init; }

    /// <summary>
    /// Estudiantes asociados al PPA con sus IDs y nombres.
    /// </summary>
    public IReadOnlyCollection<PpaStudentDto>? Students { get; init; }

    /// <summary>
    /// Fecha de creación del PPA.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha de última actualización del PPA.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
    public bool IsContinuation { get; init; }
    public bool HasContinuation { get; init; }
    public Guid? ContinuedByPpaId { get; init; }
    public Guid? ContinuationOfPpaId { get; init; }
}

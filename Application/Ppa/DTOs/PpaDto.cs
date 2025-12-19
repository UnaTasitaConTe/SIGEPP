using Domain.Ppa;

namespace Application.Ppa.DTOs;

/// <summary>
/// DTO básico de PPA para respuestas de la API.
/// Contiene información esencial sin exponer el modelo de dominio.
/// </summary>
public sealed record PpaDto
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
    /// ID del docente principal responsable del PPA.
    /// </summary>
    public required Guid PrimaryTeacherId { get; init; }

    /// <summary>
    /// Fecha de creación del PPA.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha de última actualización del PPA.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
    public string? TeacherPrimaryName { get; init; }

}

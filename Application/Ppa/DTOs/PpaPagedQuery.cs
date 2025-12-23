using Domain.Ppa;

namespace Application.Ppa.DTOs;

/// <summary>
/// Consulta paginada especializada para PPAs con filtros avanzados.
/// </summary>
public sealed record PpaPagedQuery
{
    /// <summary>
    /// Número de página a consultar (base 1). Por defecto: 1.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Cantidad de elementos por página. Por defecto: 10.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Texto de búsqueda libre para filtrar por título, objetivo general o descripción del PPA.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtro opcional por período académico.
    /// Si se especifica, solo retorna PPAs de ese período.
    /// </summary>
    public Guid? AcademicPeriodId { get; init; }

    /// <summary>
    /// Filtro opcional por estado del PPA.
    /// Valores posibles: Proposal, InProgress, Completed, Archived, InContinuing.
    /// </summary>
    public PpaStatus? Status { get; init; }

    /// <summary>
    /// Filtro opcional por docente responsable (PrimaryTeacher).
    /// Si se especifica, solo retorna PPAs donde este docente es el responsable principal.
    /// </summary>
    public Guid? ResponsibleTeacherId { get; init; }

    /// <summary>
    /// Filtro opcional por cualquier docente vinculado al PPA.
    /// Si se especifica, retorna PPAs donde este docente esté asignado
    /// (ya sea como responsable o a través de TeacherAssignments).
    /// </summary>
    public Guid? TeacherId { get; init; }
}

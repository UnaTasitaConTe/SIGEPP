using Application.Common;

namespace Application.Academics.DTOs;

/// <summary>
/// Consulta paginada especializada para asignaciones docentes con filtros adicionales.
/// </summary>
public sealed record TeacherAssignmentPagedQuery
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
    /// Texto de búsqueda libre para filtrar por nombre del docente o nombre de la asignatura.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtro opcional por estado activo/inactivo de la asignación.
    /// Si es null, no se aplica filtro por estado.
    /// Si es true, solo asignaciones activas.
    /// Si es false, solo asignaciones inactivas.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filtro opcional por período académico.
    /// Si se especifica, solo retorna asignaciones de ese período.
    /// </summary>
    public Guid? AcademicPeriodId { get; init; }

    /// <summary>
    /// Filtro opcional por docente.
    /// Si se especifica, solo retorna asignaciones de ese docente.
    /// </summary>
    public Guid? TeacherId { get; init; }

    /// <summary>
    /// Filtro opcional por asignatura.
    /// Si se especifica, solo retorna asignaciones de esa asignatura.
    /// </summary>
    public Guid? SubjectId { get; init; }
}

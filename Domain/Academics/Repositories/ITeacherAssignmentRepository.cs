using Domain.Academics.Entities;

namespace Domain.Academics.Repositories;

/// <summary>
/// Repositorio para la gestión de asignaciones docentes.
/// Define el contrato sin detalles de implementación (sin EF Core).
/// </summary>
public interface ITeacherAssignmentRepository
{
    /// <summary>
    /// Obtiene una asignación docente por su ID.
    /// </summary>
    /// <param name="id">ID de la asignación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Asignación encontrada o null si no existe.</returns>
    Task<TeacherAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las asignaciones de un docente (todos los periodos).
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones del docente.</returns>
    Task<IReadOnlyCollection<TeacherAssignment>> GetByTeacherAsync(
        Guid teacherId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las asignaciones de un docente en un período académico específico.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones del docente en el período.</returns>
    Task<IReadOnlyCollection<TeacherAssignment>> GetByTeacherAndPeriodAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las asignaciones activas de un docente en un período académico específico.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones activas del docente en el período.</returns>
    Task<IReadOnlyCollection<TeacherAssignment>> GetActiveByTeacherAndPeriodAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las asignaciones de una asignatura en un período académico específico.
    /// </summary>
    /// <param name="subjectId">ID de la asignatura.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones de la asignatura en el período.</returns>
    Task<IReadOnlyCollection<TeacherAssignment>> GetBySubjectAndPeriodAsync(
        Guid subjectId,
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las asignaciones de un período académico específico.
    /// </summary>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todas las asignaciones del período.</returns>
    Task<IReadOnlyCollection<TeacherAssignment>> GetByPeriodAsync(
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las asignaciones activas de un período académico específico.
    /// </summary>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones activas del período.</returns>
    Task<IReadOnlyCollection<TeacherAssignment>> GetActiveByPeriodAsync(
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe una asignación específica (docente-asignatura-período).
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="subjectId">ID de la asignatura.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si existe la asignación.</returns>
    Task<bool> ExistsAsync(
        Guid teacherId,
        Guid subjectId,
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe una asignación específica, excluyendo un ID particular.
    /// Útil para validaciones en actualización.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="subjectId">ID de la asignatura.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="excludeAssignmentId">ID de la asignación a excluir de la búsqueda.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si existe otra asignación con esos datos.</returns>
    Task<bool> ExistsAsync(
        Guid teacherId,
        Guid subjectId,
        Guid academicPeriodId,
        Guid excludeAssignmentId,
        CancellationToken ct = default);

    /// <summary>
    /// Agrega una nueva asignación docente al repositorio.
    /// </summary>
    /// <param name="assignment">Asignación a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(TeacherAssignment assignment, CancellationToken ct = default);

    /// <summary>
    /// Actualiza una asignación docente existente.
    /// </summary>
    /// <param name="assignment">Asignación a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateAsync(TeacherAssignment assignment, CancellationToken ct = default);

    /// <summary>
    /// Elimina una asignación docente del repositorio.
    /// </summary>
    /// <param name="assignment">Asignación a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task DeleteAsync(TeacherAssignment assignment, CancellationToken ct = default);
}

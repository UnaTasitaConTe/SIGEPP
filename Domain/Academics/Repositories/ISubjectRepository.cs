using Domain.Academics.Entities;

namespace Domain.Academics.Repositories;

/// <summary>
/// Repositorio para la gestión de asignaturas.
/// Define el contrato sin detalles de implementación (sin EF Core).
/// </summary>
public interface ISubjectRepository
{
    /// <summary>
    /// Obtiene una asignatura por su ID.
    /// </summary>
    /// <param name="id">ID de la asignatura.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Asignatura encontrada o null si no existe.</returns>
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una asignatura por su código.
    /// </summary>
    /// <param name="code">Código de la asignatura (ej: "ISW-101").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Asignatura encontrada o null si no existe.</returns>
    Task<Subject?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe una asignatura con el código especificado.
    /// </summary>
    /// <param name="code">Código a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si el código ya está en uso.</returns>
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe una asignatura con el código especificado, excluyendo un ID específico.
    /// Útil para validaciones en actualización.
    /// </summary>
    /// <param name="code">Código a verificar.</param>
    /// <param name="excludeSubjectId">ID de la asignatura a excluir de la búsqueda.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si el código ya está en uso por otra asignatura.</returns>
    Task<bool> CodeExistsAsync(string code, Guid excludeSubjectId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las asignaturas del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todas las asignaturas.</returns>
    Task<IReadOnlyCollection<Subject>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las asignaturas activas del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaturas activas.</returns>
    Task<IReadOnlyCollection<Subject>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene las asignaturas asignadas a un docente en un período académico específico.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaturas asignadas al docente en el período.</returns>
    Task<IReadOnlyCollection<Subject>> GetByTeacherAndPeriodAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Agrega una nueva asignatura al repositorio.
    /// </summary>
    /// <param name="subject">Asignatura a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(Subject subject, CancellationToken ct = default);

    /// <summary>
    /// Actualiza una asignatura existente.
    /// </summary>
    /// <param name="subject">Asignatura a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateAsync(Subject subject, CancellationToken ct = default);
}

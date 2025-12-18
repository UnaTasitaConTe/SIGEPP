using Domain.Ppa.Entities;

namespace Domain.Ppa.Repositories;

/// <summary>
/// Contrato de repositorio para la entidad <see cref="Ppa"/>.
/// Define las operaciones de persistencia y consulta de PPAs.
/// </summary>
public interface IPpaRepository
{
    /// <summary>
    /// Obtiene un PPA por su identificador único.
    /// </summary>
    /// <param name="id">ID del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El PPA si existe, null en caso contrario.</returns>
    Task<Entities.Ppa?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los PPAs asociados a un docente en un período académico específico.
    /// </summary>
    /// <param name="teacherId">ID del docente (PrimaryTeacherId).</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de PPAs del docente en el período especificado.</returns>
    Task<IReadOnlyCollection<Entities.Ppa>> GetByTeacherAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los PPAs de un período académico específico.
    /// </summary>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de todos los PPAs del período.</returns>
    Task<IReadOnlyCollection<Entities.Ppa>> GetByAcademicPeriodAsync(
        Guid academicPeriodId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los PPAs que tienen un estado específico.
    /// </summary>
    /// <param name="status">Estado del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de PPAs con el estado especificado.</returns>
    Task<IReadOnlyCollection<Entities.Ppa>> GetByStatusAsync(
        PpaStatus status,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los PPAs que incluyen una asignación docente-asignatura específica.
    /// </summary>
    /// <param name="teacherAssignmentId">ID de la asignación docente-asignatura.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de PPAs que incluyen la asignación especificada.</returns>
    Task<IReadOnlyCollection<Entities.Ppa>> GetByTeacherAssignmentAsync(
        Guid teacherAssignmentId,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un PPA activo (no Archived) para un conjunto de asignaciones docente-asignatura.
    /// </summary>
    /// <param name="teacherAssignmentIds">IDs de las asignaciones docente-asignatura.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si existe al menos un PPA activo con esas asignaciones.</returns>
    /// <remarks>
    /// Útil para validar reglas de negocio como "no crear múltiples PPAs activos
    /// para el mismo conjunto de asignaciones en el mismo período".
    /// </remarks>
    Task<bool> ExistsActiveForAssignmentsAsync(
        IEnumerable<Guid> teacherAssignmentIds,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un PPA con un título específico en un período académico.
    /// </summary>
    /// <param name="title">Título del PPA.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="excludePpaId">ID del PPA a excluir de la búsqueda (para actualizaciones).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si existe un PPA con ese título en el período.</returns>
    Task<bool> TitleExistsInPeriodAsync(
        string title,
        Guid academicPeriodId,
        Guid? excludePpaId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Agrega un nuevo PPA al repositorio.
    /// </summary>
    /// <param name="ppa">PPA a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(Entities.Ppa ppa, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un PPA existente.
    /// </summary>
    /// <param name="ppa">PPA con los datos actualizados.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateAsync(Entities.Ppa ppa, CancellationToken ct = default);
}

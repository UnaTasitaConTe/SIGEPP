using Domain.Common;
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
    /// Obtiene todos los PPAs que son continuación de un PPA específico.
    /// </summary>
    /// <param name="continuationOfPpaId">ID del PPA del cual son continuación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de PPAs que son continuación del PPA especificado.</returns>
    Task<IReadOnlyCollection<Entities.Ppa>> GetByContinuationOfAsync(
        Guid continuationOfPpaId,
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
    /// Verifica si existe un PPA con un título específico en un período académico.
    /// Este método es equivalente a <see cref="TitleExistsInPeriodAsync"/> y se proporciona
    /// para mantener compatibilidad con la nueva nomenclatura.
    /// </summary>
    /// <param name="title">Título del PPA.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="excludePpaId">ID del PPA a excluir de la búsqueda (para actualizaciones).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si existe un PPA con ese título en el período.</returns>
    /// <remarks>
    /// Este método valida la regla de negocio: no deben existir PPAs con el mismo nombre
    /// en el mismo periodo académico. Se utiliza antes de crear o actualizar un PPA.
    /// </remarks>
    Task<bool> ExistsWithTitleAsync(
        string title,
        Guid academicPeriodId,
        Guid? excludePpaId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene una lista paginada de PPAs con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1).</param>
    /// <param name="pageSize">Cantidad de elementos por página.</param>
    /// <param name="search">Texto de búsqueda opcional para filtrar por título, objetivo general o descripción.</param>
    /// <param name="academicPeriodId">Filtro opcional por período académico.</param>
    /// <param name="status">Filtro opcional por estado del PPA.</param>
    /// <param name="responsibleTeacherId">Filtro opcional por docente responsable.</param>
    /// <param name="teacherId">Filtro opcional por cualquier docente vinculado (responsable o asignado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado paginado con PPAs y metadatos de paginación.</returns>
    Task<PagedResult<Entities.Ppa>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        Guid? academicPeriodId = null,
        PpaStatus? status = null,
        Guid? responsibleTeacherId = null,
        Guid? teacherId = null,
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

    /// <summary>
    /// Actualiza solo la información básica del PPA (título, objetivos, descripción, estado, etc.).
    /// Realiza su propia lectura de la entidad PpaEntity y SaveChanges independiente.
    /// </summary>
    /// <param name="ppaId">ID del PPA a actualizar.</param>
    /// <param name="title">Nuevo título.</param>
    /// <param name="generalObjective">Nuevo objetivo general.</param>
    /// <param name="specificObjectives">Nuevos objetivos específicos.</param>
    /// <param name="description">Nueva descripción.</param>
    /// <param name="status">Nuevo estado.</param>
    /// <param name="primaryTeacherId">Nuevo docente principal.</param>
    /// <param name="continuationOfPpaId">ID del PPA del que es continuación.</param>
    /// <param name="continuedByPpaId">ID del PPA que continúa este.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateBasicInfoAsync(
        Guid ppaId,
        string title,
        string? generalObjective,
        string? specificObjectives,
        string? description,
        PpaStatus status,
        Guid primaryTeacherId,
        Guid? continuationOfPpaId,
        Guid? continuedByPpaId,
        CancellationToken ct = default);

    /// <summary>
    /// Actualiza solo las asignaciones docente-asignatura del PPA.
    /// Realiza su propia lectura de PpaTeacherAssignmentEntity y SaveChanges independiente.
    /// </summary>
    /// <param name="ppaId">ID del PPA a actualizar.</param>
    /// <param name="teacherAssignmentIds">Nueva lista de IDs de asignaciones docente-asignatura.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateTeacherAssignmentsAsync(
        Guid ppaId,
        IEnumerable<Guid> teacherAssignmentIds,
        CancellationToken ct = default);

    /// <summary>
    /// Actualiza solo los estudiantes del PPA.
    /// Realiza su propia lectura de PpaStudentEntity y SaveChanges independiente.
    /// </summary>
    /// <param name="ppaId">ID del PPA a actualizar.</param>
    /// <param name="students">Nueva lista de estudiantes con sus IDs y nombres.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateStudentsAsync(
        Guid ppaId,
        IEnumerable<(Guid? Id, string Name)> students,
        CancellationToken ct = default);
}

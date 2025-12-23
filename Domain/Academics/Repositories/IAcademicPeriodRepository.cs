using Domain.Academics.Entities;
using Domain.Common;

namespace Domain.Academics.Repositories;

/// <summary>
/// Repositorio para la gestión de períodos académicos.
/// Define el contrato sin detalles de implementación (sin EF Core).
/// </summary>
public interface IAcademicPeriodRepository
{
    /// <summary>
    /// Obtiene un período académico por su ID.
    /// </summary>
    /// <param name="id">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Período académico encontrado o null si no existe.</returns>
    Task<AcademicPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un período académico por su código.
    /// </summary>
    /// <param name="code">Código del período académico (ej: "2024-1").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Período académico encontrado o null si no existe.</returns>
    Task<AcademicPeriod?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un período académico con el código especificado.
    /// </summary>
    /// <param name="code">Código a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si el código ya está en uso.</returns>
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un período académico con el código especificado, excluyendo un ID específico.
    /// Útil para validaciones en actualización.
    /// </summary>
    /// <param name="code">Código a verificar.</param>
    /// <param name="excludePeriodId">ID del período a excluir de la búsqueda.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si el código ya está en uso por otro período.</returns>
    Task<bool> CodeExistsAsync(string code, Guid excludePeriodId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los períodos académicos del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todos los períodos académicos.</returns>
    Task<IReadOnlyCollection<AcademicPeriod>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los períodos académicos activos del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de períodos académicos activos.</returns>
    Task<IReadOnlyCollection<AcademicPeriod>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene el período académico vigente en una fecha específica.
    /// </summary>
    /// <param name="date">Fecha a evaluar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Período académico vigente en la fecha o null si no hay ninguno.</returns>
    Task<AcademicPeriod?> GetCurrentAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el período académico vigente hoy.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Período académico vigente hoy o null si no hay ninguno.</returns>
    Task<AcademicPeriod?> GetCurrentTodayAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene una lista paginada de períodos académicos con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1).</param>
    /// <param name="pageSize">Cantidad de elementos por página.</param>
    /// <param name="search">Texto de búsqueda opcional para filtrar por código o nombre.</param>
    /// <param name="isActive">Filtro opcional por estado activo (true), inactivo (false), o todos (null).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado paginado con períodos académicos y metadatos de paginación.</returns>
    Task<PagedResult<AcademicPeriod>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken ct = default);

    /// <summary>
    /// Agrega un nuevo período académico al repositorio.
    /// </summary>
    /// <param name="period">Período académico a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(AcademicPeriod period, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un período académico existente.
    /// </summary>
    /// <param name="period">Período académico a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateAsync(AcademicPeriod period, CancellationToken ct = default);
}

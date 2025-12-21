using Domain.Ppa.Entities;

namespace Domain.Ppa.Repositories;

/// <summary>
/// Contrato de repositorio para el historial de PPAs.
/// Define las operaciones de persistencia y consulta del historial de cambios.
/// </summary>
public interface IPpaHistoryRepository
{
    /// <summary>
    /// Agrega una nueva entrada al historial de un PPA.
    /// </summary>
    /// <param name="entry">Entrada de historial a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(PpaHistoryEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Agrega múltiples entradas al historial de un PPA de forma atómica.
    /// </summary>
    /// <param name="entries">Colección de entradas a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <remarks>
    /// Útil cuando una operación compleja genera múltiples entradas de historial
    /// que deben ser guardadas juntas (ej: continuación de un PPA que actualiza
    /// múltiples campos).
    /// </remarks>
    Task AddRangeAsync(IEnumerable<PpaHistoryEntry> entries, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las entradas de historial de un PPA específico,
    /// ordenadas por fecha descendente (más recientes primero).
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de entradas de historial del PPA.</returns>
    Task<IReadOnlyCollection<PpaHistoryEntry>> GetByPpaAsync(
        Guid ppaId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene las entradas de historial de un PPA filtradas por tipo de acción.
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="actionType">Tipo de acción a filtrar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de entradas del tipo especificado.</returns>
    Task<IReadOnlyCollection<PpaHistoryEntry>> GetByPpaAndActionTypeAsync(
        Guid ppaId,
        PpaHistoryActionType actionType,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las entradas de historial realizadas por un usuario específico.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de entradas de historial realizadas por el usuario.</returns>
    /// <remarks>
    /// Útil para auditorías y seguimiento de acciones de usuarios específicos.
    /// </remarks>
    Task<IReadOnlyCollection<PpaHistoryEntry>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default);
}

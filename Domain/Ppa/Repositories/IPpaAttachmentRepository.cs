using Domain.Ppa.Entities;

namespace Domain.Ppa.Repositories;

/// <summary>
/// Contrato de repositorio para la entidad <see cref="PpaAttachment"/>.
/// Define las operaciones de persistencia y consulta de anexos de PPA.
/// </summary>
public interface IPpaAttachmentRepository
{
    /// <summary>
    /// Obtiene un anexo por su identificador único.
    /// </summary>
    /// <param name="id">ID del anexo.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El anexo si existe, null en caso contrario.</returns>
    Task<PpaAttachment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los anexos de un PPA, incluyendo los eliminados.
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="includeDeleted">Si es true, incluye anexos marcados como eliminados.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de anexos del PPA.</returns>
    Task<IReadOnlyCollection<PpaAttachment>> GetByPpaAsync(
        Guid ppaId,
        bool includeDeleted = false,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los anexos de un PPA filtrados por tipo.
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="type">Tipo de anexo.</param>
    /// <param name="includeDeleted">Si es true, incluye anexos marcados como eliminados.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de anexos del PPA del tipo especificado.</returns>
    Task<IReadOnlyCollection<PpaAttachment>> GetByPpaAndTypeAsync(
        Guid ppaId,
        PpaAttachmentType type,
        bool includeDeleted = false,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los anexos subidos por un usuario específico.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="includeDeleted">Si es true, incluye anexos marcados como eliminados.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Colección de anexos subidos por el usuario.</returns>
    Task<IReadOnlyCollection<PpaAttachment>> GetByUserAsync(
        Guid userId,
        bool includeDeleted = false,
        CancellationToken ct = default);

    /// <summary>
    /// Cuenta cuántos anexos de un tipo específico tiene un PPA.
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="type">Tipo de anexo.</param>
    /// <param name="includeDeleted">Si es true, incluye anexos marcados como eliminados.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Cantidad de anexos del tipo especificado.</returns>
    /// <remarks>
    /// Útil para validar reglas como "un PPA debe tener al menos un PpaDocument".
    /// </remarks>
    Task<int> CountByTypeAsync(
        Guid ppaId,
        PpaAttachmentType type,
        bool includeDeleted = false,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un anexo con una clave de archivo específica.
    /// </summary>
    /// <param name="fileKey">Clave del archivo.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si existe un anexo con esa clave de archivo.</returns>
    Task<bool> FileKeyExistsAsync(string fileKey, CancellationToken ct = default);

    /// <summary>
    /// Agrega un nuevo anexo al repositorio.
    /// </summary>
    /// <param name="attachment">Anexo a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(PpaAttachment attachment, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un anexo existente.
    /// </summary>
    /// <param name="attachment">Anexo con los datos actualizados.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateAsync(PpaAttachment attachment, CancellationToken ct = default);

    /// <summary>
    /// Elimina permanentemente un anexo del repositorio (hard delete).
    /// </summary>
    /// <param name="attachment">Anexo a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <remarks>
    /// Este método elimina físicamente el registro.
    /// Para eliminación lógica, use el método <see cref="PpaAttachment.MarkAsDeleted"/>.
    /// </remarks>
    Task DeleteAsync(PpaAttachment attachment, CancellationToken ct = default);
}

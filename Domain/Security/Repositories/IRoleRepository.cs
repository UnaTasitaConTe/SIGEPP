using Domain.Security.Entities;

namespace Domain.Security.Repositories;

/// <summary>
/// Contrato para el repositorio de roles.
/// Define las operaciones de persistencia para la entity Role.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Obtiene un rol por su código único
    /// </summary>
    Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un rol por su ID
    /// </summary>
    Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los roles del sistema
    /// </summary>
    Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un rol con el código especificado
    /// </summary>
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega un nuevo rol al sistema
    /// </summary>
    Task AddAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un rol (solo si no es rol del sistema)
    /// </summary>
    Task DeleteAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda los cambios pendientes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

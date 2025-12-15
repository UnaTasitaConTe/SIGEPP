using Domain.Security.ValueObjects;

namespace Domain.Security.Repositories;

/// <summary>
/// Contrato para el repositorio de permisos.
/// Define las operaciones de consulta para permisos del sistema.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Obtiene un permiso por su código único (ej: "ppa.create")
    /// </summary>
    Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los permisos del sistema
    /// </summary>
    Task<IReadOnlyCollection<Permission>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los permisos de un módulo específico (ej: "ppa", "subjects")
    /// </summary>
    Task<IReadOnlyCollection<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un permiso con el código especificado
    /// </summary>
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);
}

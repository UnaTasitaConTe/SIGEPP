using Domain.Common;

namespace Domain.Users;

/// <summary>
/// Repositorio para la gestión de usuarios.
/// Define el contrato sin detalles de implementación (sin EF Core).
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuario encontrado o null si no existe.</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un usuario por su email.
    /// </summary>
    /// <param name="email">Email del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuario encontrado o null si no existe.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un usuario con el email especificado.
    /// </summary>
    /// <param name="email">Email a verificar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si el email ya está en uso.</returns>
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un usuario con el email especificado, excluyendo un ID específico.
    /// Útil para validaciones en actualización.
    /// </summary>
    /// <param name="email">Email a verificar.</param>
    /// <param name="excludeUserId">ID del usuario a excluir de la búsqueda.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si el email ya está en uso por otro usuario.</returns>
    Task<bool> EmailExistsAsync(string email, Guid excludeUserId, CancellationToken ct = default);

    /// <summary>
    /// Agrega un nuevo usuario al repositorio.
    /// </summary>
    /// <param name="user">Usuario a agregar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    /// <param name="user">Usuario a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UpdateAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los usuarios activos del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de usuarios activos.</returns>
    Task<IReadOnlyCollection<User>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los usuarios del sistema (activos e inactivos).
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todos los usuarios.</returns>
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1).</param>
    /// <param name="pageSize">Cantidad de elementos por página.</param>
    /// <param name="search">Texto de búsqueda opcional para filtrar por nombre o email.</param>
    /// <param name="isActive">Filtro opcional por estado activo (true), inactivo (false), o todos (null).</param>
    /// <param name="roleCode">Filtro opcional por código de rol (ej: "ADMIN", "DOCENTE").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado paginado con usuarios y metadatos de paginación.</returns>
    Task<PagedResult<User>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        string? roleCode = null,
        CancellationToken ct = default);
}

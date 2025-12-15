namespace Application.Security;

/// <summary>
/// Servicio para generación y validación de tokens JWT.
/// La implementación concreta se encuentra en la capa de Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Genera un token JWT para un usuario autenticado.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="name">Nombre del usuario.</param>
    /// <param name="email">Email del usuario.</param>
    /// <param name="roles">Códigos de roles del usuario.</param>
    /// <param name="permissions">Permisos efectivos del usuario.</param>
    /// <returns>Token JWT firmado.</returns>
    string GenerateToken(
        Guid userId,
        string name,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> permissions);

    /// <summary>
    /// Genera un token JWT a partir de un objeto CurrentUser.
    /// </summary>
    /// <param name="user">Usuario autenticado.</param>
    /// <returns>Token JWT firmado.</returns>
    string GenerateToken(CurrentUser user);
}

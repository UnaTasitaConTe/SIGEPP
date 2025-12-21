namespace Application.Security;

/// <summary>
/// Servicio para obtener la información del usuario autenticado actualmente.
/// </summary>
/// <remarks>
/// Esta interfaz abstrae el acceso al usuario autenticado, permitiendo que la capa
/// de aplicación no dependa directamente de infraestructura HTTP (IHttpContextAccessor).
/// La implementación concreta se encuentra en la capa de Infrastructure.
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtiene la información del usuario autenticado actualmente.
    /// </summary>
    /// <returns>
    /// Información del usuario autenticado, o null si no hay un usuario autenticado
    /// (por ejemplo, en endpoints anónimos).
    /// </returns>
    CurrentUser? GetCurrentUser();

    /// <summary>
    /// Obtiene el ID del usuario autenticado actualmente.
    /// </summary>
    /// <returns>ID del usuario autenticado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si no hay un usuario autenticado actualmente.
    /// </exception>
    Guid GetCurrentUserId();
}

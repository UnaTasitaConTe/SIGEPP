using Microsoft.AspNetCore.Authorization;

namespace SIGEPP.Authorization;

/// <summary>
/// Requirement de autorización basado en permisos.
/// Representa la necesidad de que el usuario autenticado posea un permiso específico.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Código del permiso requerido (ej: "users.view", "ppa.create").
    /// </summary>
    public string PermissionCode { get; }

    /// <summary>
    /// Crea una instancia de PermissionRequirement.
    /// </summary>
    /// <param name="permissionCode">Código del permiso requerido.</param>
    /// <exception cref="ArgumentException">Si el código de permiso es nulo o vacío.</exception>
    public PermissionRequirement(string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            throw new ArgumentException("El código de permiso no puede ser nulo o vacío.", nameof(permissionCode));

        PermissionCode = permissionCode;
    }
}

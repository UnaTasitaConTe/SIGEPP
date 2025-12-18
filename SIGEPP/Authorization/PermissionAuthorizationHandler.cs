using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SIGEPP.Authorization;

/// <summary>
/// Handler de autorización que verifica si el usuario autenticado posee un permiso específico.
/// Los permisos se verifican contra los claims del tipo "permission" en el token JWT.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Maneja el requirement de permiso, verificando si el usuario tiene el permiso requerido.
    /// </summary>
    /// <param name="context">Contexto de autorización.</param>
    /// <param name="requirement">Requirement de permiso a verificar.</param>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Verificar que el usuario esté autenticado
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning(
                "Usuario no autenticado intentó acceder a recurso que requiere permiso '{Permission}'",
                requirement.PermissionCode);
            return Task.CompletedTask;
        }

        // Obtener todos los claims de tipo "permission"
        var permissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        // Verificar si el usuario tiene el permiso requerido
        if (permissions.Contains(requirement.PermissionCode, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogDebug(
                "Usuario '{UserId}' tiene permiso '{Permission}'",
                GetUserId(context.User),
                requirement.PermissionCode);

            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "Usuario '{UserId}' no tiene permiso '{Permission}'. Permisos del usuario: [{Permissions}]",
                GetUserId(context.User),
                requirement.PermissionCode,
                string.Join(", ", permissions));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Obtiene el ID del usuario desde los claims.
    /// </summary>
    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? "Unknown";
    }
}

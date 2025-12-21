using Application.Security;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Security;

/// <summary>
/// Implementación de ICurrentUserService usando IHttpContextAccessor.
/// Extrae la información del usuario autenticado desde el contexto HTTP.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public CurrentUser? GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var user = httpContext.User;

        // Extraer userId del claim
        var userIdClaim = user.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        // Extraer name del claim
        var name = user.FindFirst("name")?.Value ?? string.Empty;

        // Extraer email del claim
        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        // Extraer roles
        var roles = user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();

        // Extraer permissions
        var permissions = user.FindAll("permission")
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();

        return new CurrentUser
        {
            UserId = userId,
            Name = name,
            Email = email,
            Roles = roles,
            Permissions = permissions
        };
    }

    public Guid GetCurrentUserId()
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
            throw new InvalidOperationException(
                "No hay un usuario autenticado en el contexto actual. " +
                "Este método solo debe llamarse desde endpoints que requieren autenticación.");

        return currentUser.UserId;
    }
}

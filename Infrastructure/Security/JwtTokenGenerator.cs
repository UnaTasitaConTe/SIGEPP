using Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Security;

/// <summary>
/// Implementación de IJwtTokenGenerator usando System.IdentityModel.Tokens.Jwt.
/// Genera tokens JWT firmados con HS256 (HMAC-SHA256).
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _jwtOptions.Validate();
    }

    /// <summary>
    /// Genera un token JWT con todos los claims del usuario.
    /// </summary>
    public string GenerateToken(
        Guid userId,
        string name,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            // Claims estándar JWT
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),      // Subject (user ID)
            new(JwtRegisteredClaimNames.Email, email),                 // Email
            new(JwtRegisteredClaimNames.Name, name),                   // Nombre completo
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID (único por token)
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Issued At

            // Claims personalizados
            new("userId", userId.ToString()),
        };

        // Agregar roles como claims individuales
        foreach (var role in roles ?? [])
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Agregar permisos como claims individuales
        foreach (var permission in permissions ?? [])
        {
            claims.Add(new Claim("permission", permission));
        }

        // Crear clave de firma simétrica
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Calcular expiración
        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        // Crear el token
        var securityToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: signingCredentials);

        // Serializar el token a string
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(securityToken);
    }

    /// <summary>
    /// Genera un token JWT a partir de un objeto CurrentUser.
    /// </summary>
    public string GenerateToken(CurrentUser user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return GenerateToken(
            userId: user.UserId,
            name: user.Name,
            email: user.Email,
            roles: user.Roles,
            permissions: user.Permissions);
    }
}

namespace Infrastructure.Security;

/// <summary>
/// Opciones de configuración para JWT (JSON Web Tokens).
/// Estos valores se cargan desde appsettings.json en la sección "Jwt".
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Nombre de la sección en appsettings.json.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Clave secreta para firmar tokens JWT.
    /// DEBE ser una cadena larga y segura (mínimo 32 caracteres).
    /// NUNCA compartir esta clave ni commitearla en repositorios públicos.
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// Emisor del token (iss claim).
    /// Identifica quién emitió el token.
    /// Ejemplo: "SIGEPP-API" o "https://api.sigepp.fesc.edu.co"
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Audiencia del token (aud claim).
    /// Identifica para quién está destinado el token.
    /// Ejemplo: "SIGEPP-WebApp" o "https://app.sigepp.fesc.edu.co"
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Duración del token en minutos.
    /// Después de este tiempo, el token expira y el usuario debe autenticarse nuevamente.
    /// Valor recomendado: 60 minutos (1 hora) para aplicaciones web.
    /// </summary>
    public int ExpirationMinutes { get; init; } = 60;

    /// <summary>
    /// Valida que las opciones sean correctas.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("JWT:SecretKey no está configurado en appsettings.json");

        if (SecretKey.Length < 32)
            throw new InvalidOperationException("JWT:SecretKey debe tener al menos 32 caracteres para ser seguro");

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT:Issuer no está configurado en appsettings.json");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT:Audience no está configurado en appsettings.json");

        if (ExpirationMinutes <= 0)
            throw new InvalidOperationException("JWT:ExpirationMinutes debe ser mayor a 0");
    }
}

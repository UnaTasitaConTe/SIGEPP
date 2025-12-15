using Application.Security;
using BCrypt.Net;

namespace Infrastructure.Security;

/// <summary>
/// Implementación de IPasswordHasher usando BCrypt.
/// BCrypt es un algoritmo de hashing adaptativo diseñado para contraseñas.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    // Work factor de BCrypt (número de iteraciones = 2^12 = 4096)
    // Mayor work factor = más seguro pero más lento
    // 12 es un buen balance entre seguridad y rendimiento
    private const int WorkFactor = 12;

    /// <summary>
    /// Genera un hash BCrypt a partir de una contraseña en texto plano.
    /// </summary>
    /// <param name="rawPassword">Contraseña en texto plano.</param>
    /// <returns>Hash BCrypt de la contraseña.</returns>
    /// <exception cref="ArgumentException">Si la contraseña está vacía.</exception>
    public string HashPassword(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(rawPassword));

        return BCrypt.Net.BCrypt.HashPassword(rawPassword, WorkFactor);
    }

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con un hash BCrypt.
    /// </summary>
    /// <param name="rawPassword">Contraseña en texto plano.</param>
    /// <param name="passwordHash">Hash BCrypt a verificar.</param>
    /// <returns>True si la contraseña coincide con el hash.</returns>
    public bool VerifyPassword(string rawPassword, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            return false;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(rawPassword, passwordHash);
        }
        catch (SaltParseException)
        {
            // Hash inválido o corrupto
            return false;
        }
    }
}

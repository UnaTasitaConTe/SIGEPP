namespace Application.Security;

/// <summary>
/// Servicio para el hashing y verificación de contraseñas.
/// La implementación concreta se encuentra en la capa de Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Genera un hash seguro a partir de una contraseña en texto plano.
    /// </summary>
    /// <param name="rawPassword">Contraseña en texto plano.</param>
    /// <returns>Hash de la contraseña.</returns>
    string HashPassword(string rawPassword);

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con un hash.
    /// </summary>
    /// <param name="rawPassword">Contraseña en texto plano.</param>
    /// <param name="passwordHash">Hash de contraseña a verificar.</param>
    /// <returns>True si la contraseña es correcta.</returns>
    bool VerifyPassword(string rawPassword, string passwordHash);
}

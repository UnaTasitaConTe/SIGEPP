using Application.Auth.Commands;
using Application.Auth.DTOs;
using Application.Security;
using Domain.Security;
using Domain.Users;
namespace Application.Auth;

/// <summary>
/// Servicio de aplicación para autenticación de usuarios.
/// Maneja login, generación de tokens y validación de credenciales.
/// </summary>
public sealed class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator
       )
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
    }

    /// <summary>
    /// Autentica un usuario y genera un token JWT.
    /// </summary>
    /// <param name="command">Comando con email y contraseña.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado con el token JWT y datos del usuario.</returns>
    /// <exception cref="UnauthorizedAccessException">Si las credenciales son inválidas.</exception>
    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Buscar usuario por email
        var user = await _userRepository.GetByEmailAsync(command.Email, ct);

        if (user == null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // Verificar que el usuario esté activo
        if (!user.IsActive)
            throw new UnauthorizedAccessException("El usuario está inactivo.");

        // Verificar contraseña
        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // Generar token JWT
        var token = GenerateTokenForUser(user);

        return new LoginResult
        {
            Token = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Roles = user.Roles.Select(r => r.Code).ToList(),
            ExpiresAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Genera un token JWT para un usuario.
    /// </summary>
    private string GenerateTokenForUser(User user)
    {
        var currentUser = new CurrentUser
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Roles = [.. user.Roles.Select(r => r.Code)],
            Permissions = [.. user.GetAllPermissions().Select(p => p.Value)]
        };

        return _jwtTokenGenerator.GenerateToken(currentUser);
    }
}

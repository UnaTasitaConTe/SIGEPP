using Application.Auth;
using Application.Auth.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador de autenticación para SIGEPP.
/// Maneja login y generación de tokens JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Autentica un usuario y genera un token JWT.
    /// </summary>
    /// <param name="command">Credenciales del usuario (email y contraseña).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Token JWT y datos del usuario autenticado.</returns>
    /// <response code="200">Login exitoso. Retorna el token JWT.</response>
    /// <response code="401">Credenciales inválidas o usuario inactivo.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    [HttpPost("login")]
    [AllowAnonymous] // Endpoint público - no requiere autenticación
    [ProducesResponseType(typeof(Application.Auth.DTOs.LoginResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(command, ct);

            _logger.LogInformation(
                "Usuario {Email} autenticado exitosamente. UserId: {UserId}",
                command.Email,
                result.UserId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                "Intento de login fallido para email: {Email}. Razón: {Reason}",
                command.Email,
                ex.Message);

            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado durante login para email: {Email}",
                command.Email);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Ocurrió un error durante la autenticación." });
        }
    }

    /// <summary>
    /// Obtiene información del usuario autenticado actualmente.
    /// </summary>
    /// <returns>Información del usuario extraída del token JWT.</returns>
    /// <response code="200">Retorna la información del usuario autenticado.</response>
    /// <response code="401">Token inválido o expirado.</response>
    [Authorize] // Requiere autenticación
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        // Extraer información del token JWT (claims)
        var userId = User.FindFirst("userId")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = User.FindFirst("name")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
        var permissions = User.FindAll("permission")
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            userId,
            email,
            name,
            roles,
            permissions
        });
    }
}

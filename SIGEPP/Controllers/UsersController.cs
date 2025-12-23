using Application.Users;
using Application.Users.Commands;
using Application.Users.DTOs;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador para gestión de usuarios en SIGEPP.
/// Todos los endpoints requieren autenticación.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles="ADMIN")] // Todos los endpoints requieren autenticación
public class UsersController : ControllerBase
{
    private readonly UserAppService _userAppService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserAppService userAppService,
        ILogger<UsersController> logger)
    {
        _userAppService = userAppService ?? throw new ArgumentNullException(nameof(userAppService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene la lista de todos los usuarios del sistema.
    /// </summary>
    /// <param name="activeOnly">Si es true, solo retorna usuarios activos. Default: false.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de usuarios.</returns>
    /// <response code="200">Lista de usuarios obtenida exitosamente.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        try
        {
            var users = activeOnly
                ? await _userAppService.GetAllActiveUsersAsync(ct)
                : await _userAppService.GetAllUsersAsync(ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} usuarios (activeOnly: {ActiveOnly})",
                users.Count,
                activeOnly);

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de usuarios");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener lista de usuarios." });
        }
    }

    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtros opcionales.
    /// </summary>
    /// <param name="page">Número de página (base 1). Default: 1.</param>
    /// <param name="pageSize">Cantidad de elementos por página. Default: 10.</param>
    /// <param name="search">Texto de búsqueda opcional para filtrar por nombre o email.</param>
    /// <param name="isActive">Filtro opcional por estado activo (true/false).</param>
    /// <param name="roleCode">Filtro opcional por código de rol (ej: "ADMIN", "DOCENTE").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado paginado con usuarios.</returns>
    /// <response code="200">Lista paginada obtenida exitosamente.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? roleCode = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = new UserPagedQuery
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                IsActive = isActive,
                RoleCode = roleCode
            };

            var result = await _userAppService.GetUsersPagedAsync(query, ct);

            _logger.LogInformation(
                "Se obtuvo página {Page} de usuarios. Total: {TotalItems}, Retornados: {Count}",
                page,
                result.TotalItems,
                result.Items.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista paginada de usuarios");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener lista paginada de usuarios." });
        }
    }

    /// <summary>
    /// Obtiene un usuario por su ID con información detallada.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Información detallada del usuario.</returns>
    /// <response code="200">Usuario encontrado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
    {
        try
        {
            var user = await _userAppService.GetUserByIdAsync(id, ct);

            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado", id);
                return NotFound(new { message = $"Usuario con ID '{id}' no encontrado." });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {UserId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener el usuario." });
        }
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="command">Datos del usuario a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del usuario creado.</returns>
    /// <response code="201">Usuario creado exitosamente.</response>
    /// <response code="400">Datos inválidos o email ya en uso.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command,
        CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = await _userAppService.CreateUserAsync(command, ct);

            _logger.LogInformation(
                "Usuario creado exitosamente. UserId: {UserId}, Email: {Email}",
                userId,
                command.Email);

            return CreatedAtAction(
                nameof(GetById),
                new { id = userId },
                new { id = userId, message = "Usuario creado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al crear usuario. Email: {Email}. Razón: {Reason}",
                command.Email,
                ex.Message);

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear usuario. Email: {Email}", command.Email);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al crear el usuario." });
        }
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <param name="id">ID del usuario a actualizar.</param>
    /// <param name="command">Nuevos datos del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de actualización.</returns>
    /// <response code="200">Usuario actualizado exitosamente.</response>
    /// <response code="400">Datos inválidos o email ya en uso.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserCommand command,
        CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.Id != id)
                return Conflict("El id no coincide con el body");

            // Asegurar que el ID del comando coincida con el de la ruta
            await _userAppService.UpdateUserAsync(command, ct);

            _logger.LogInformation("Usuario {UserId} actualizado exitosamente", id);

            return Ok(new { message = "Usuario actualizado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al actualizar usuario {UserId}. Razón: {Reason}", id, ex.Message);

            if (ex.Message.Contains("no existe"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar usuario {UserId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al actualizar el usuario." });
        }
    }

    /// <summary>
    /// Activa un usuario previamente desactivado.
    /// </summary>
    /// <param name="id">ID del usuario a activar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de activación.</returns>
    /// <response code="200">Usuario activado exitosamente.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken ct)
    {
        try
        {
            await _userAppService.ActivateUserAsync(id, ct);

            _logger.LogInformation("Usuario {UserId} activado", id);

            return Ok(new { message = "Usuario activado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al activar usuario {UserId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al activar usuario {UserId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al activar el usuario." });
        }
    }

    /// <summary>
    /// Desactiva un usuario. El usuario no podrá autenticarse mientras esté desactivado.
    /// </summary>
    /// <param name="id">ID del usuario a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de desactivación.</returns>
    /// <response code="200">Usuario desactivado exitosamente.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken ct)
    {
        try
        {
            await _userAppService.DeactivateUserAsync(id, ct);

            _logger.LogInformation("Usuario {UserId} desactivado", id);

            return Ok(new { message = "Usuario desactivado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al desactivar usuario {UserId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al desactivar usuario {UserId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al desactivar el usuario." });
        }
    }

    /// <summary>
    /// Asigna uno o más roles a un usuario.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="request">Códigos de roles a asignar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de asignación.</returns>
    /// <response code="200">Roles asignados exitosamente.</response>
    /// <response code="400">Algún rol no existe.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignRoles(
        Guid id,
        [FromBody] AssignRolesRequest request,
        CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userAppService.AssignRolesAsync(id, request.RoleCodes, ct);

            _logger.LogInformation(
                "Roles asignados a usuario {UserId}. Roles: {Roles}",
                id,
                string.Join(", ", request.RoleCodes));

            return Ok(new { message = "Roles asignados exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al asignar roles a usuario {UserId}. Razón: {Reason}", id, ex.Message);

            if (ex.Message.Contains("no existe"))
            {
                if (ex.Message.Contains("Usuario"))
                    return NotFound(new { message = ex.Message });
                else
                    return BadRequest(new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al asignar roles a usuario {UserId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al asignar roles." });
        }
    }

    /// <summary>
    /// Quita un rol de un usuario.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="roleCode">Código del rol a quitar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de eliminación.</returns>
    /// <response code="200">Rol quitado exitosamente.</response>
    /// <response code="400">Rol no existe.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpDelete("{id:guid}/roles/{roleCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveRole(
        Guid id,
        string roleCode,
        CancellationToken ct)
    {
        try
        {
            await _userAppService.RemoveRoleAsync(id, roleCode, ct);

            _logger.LogInformation("Rol {RoleCode} quitado del usuario {UserId}", roleCode, id);

            return Ok(new { message = $"Rol '{roleCode}' quitado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al quitar rol {RoleCode} del usuario {UserId}. Razón: {Reason}",
                roleCode,
                id,
                ex.Message);

            if (ex.Message.Contains("Usuario") && ex.Message.Contains("no existe"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al quitar rol {RoleCode} del usuario {UserId}",
                roleCode,
                id);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al quitar el rol." });
        }
    }

    /// <summary>
    /// Cambia la contraseña de un usuario.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="request">Nueva contraseña.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de cambio.</returns>
    /// <response code="200">Contraseña cambiada exitosamente.</response>
    /// <response code="400">Contraseña inválida.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPut("{id:guid}/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        Guid id,
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userAppService.ChangePasswordAsync(id, request.NewPassword, ct);

            _logger.LogInformation("Contraseña cambiada para usuario {UserId}", id);

            return Ok(new { message = "Contraseña cambiada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al cambiar contraseña del usuario {UserId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cambiar contraseña del usuario {UserId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al cambiar la contraseña." });
        }
    }

    /// <summary>
    /// Obtiene todos los permisos efectivos de un usuario.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de códigos de permisos.</returns>
    /// <response code="200">Permisos obtenidos exitosamente.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("{id:guid}/permissions")]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPermissions(
        Guid id,
        CancellationToken ct)
    {
        try
        {
            var permissions = await _userAppService.GetUserPermissionsAsync(id, ct);

            return Ok(new { permissions });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al obtener permisos del usuario {UserId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener permisos del usuario {UserId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener los permisos." });
        }
    }
}

// ====================================
// DTOs de Request para endpoints
// ====================================

/// <summary>
/// Request para asignar roles a un usuario.
/// </summary>
public sealed record AssignRolesRequest
{
    /// <summary>
    /// Códigos de roles a asignar (ej: ["ADMIN", "DOCENTE"]).
    /// </summary>
    [Required(ErrorMessage = "Se debe especificar al menos un rol.")]
    [MinLength(1, ErrorMessage = "Se debe especificar al menos un rol.")]
    public required IEnumerable<string> RoleCodes { get; init; }
}

/// <summary>
/// Request para cambiar la contraseña de un usuario.
/// </summary>
public sealed record ChangePasswordRequest
{
    /// <summary>
    /// Nueva contraseña en texto plano.
    /// </summary>
    [Required(ErrorMessage = "La nueva contraseña es requerida.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public required string NewPassword { get; init; }
}

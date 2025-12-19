using Application.Academics;
using Application.Academics.Commands;
using Application.Academics.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador para gestión de períodos académicos en SIGEPP.
/// Todos los endpoints requieren autenticación con rol ADMIN.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AcademicPeriodsController : ControllerBase
{
    private readonly AcademicPeriodsAppService _academicPeriodsAppService;
    private readonly ILogger<AcademicPeriodsController> _logger;

    public AcademicPeriodsController(
        AcademicPeriodsAppService academicPeriodsAppService,
        ILogger<AcademicPeriodsController> logger)
    {
        _academicPeriodsAppService = academicPeriodsAppService ?? throw new ArgumentNullException(nameof(academicPeriodsAppService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene la lista de todos los períodos académicos del sistema.
    /// </summary>
    /// <param name="activeOnly">Si es true, solo retorna períodos activos. Default: false.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de períodos académicos.</returns>
    /// <response code="200">Lista de períodos obtenida exitosamente.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet]
    [Authorize(Policy = "Periods.View")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AcademicPeriodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        try
        {
            var periods = activeOnly
                ? await _academicPeriodsAppService.GetActiveAsync(ct)
                : await _academicPeriodsAppService.GetAllAsync(ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} períodos académicos (activeOnly: {ActiveOnly})",
                periods.Count,
                activeOnly);

            return Ok(periods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de períodos académicos");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener lista de períodos académicos." });
        }
    }

    /// <summary>
    /// Obtiene un período académico por su ID con información detallada.
    /// </summary>
    /// <param name="id">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Información detallada del período académico.</returns>
    /// <response code="200">Período académico encontrado.</response>
    /// <response code="404">Período académico no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Periods.View")]
    [ProducesResponseType(typeof(AcademicPeriodDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            var period = await _academicPeriodsAppService.GetByIdAsync(id, ct);

            if (period == null)
            {
                _logger.LogWarning("Período académico con ID {PeriodId} no encontrado", id);
                return NotFound(new { message = $"Período académico con ID '{id}' no encontrado." });
            }

            return Ok(period);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener período académico {PeriodId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener el período académico." });
        }
    }

    /// <summary>
    /// Crea un nuevo período académico en el sistema.
    /// </summary>
    /// <param name="command">Datos del período académico a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del período académico creado.</returns>
    /// <response code="201">Período académico creado exitosamente.</response>
    /// <response code="400">Datos inválidos o código ya en uso.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost]
    [Authorize(Policy = "Periods.Create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAcademicPeriodCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var periodId = await _academicPeriodsAppService.CreateAsync(command, ct);

            _logger.LogInformation(
                "Período académico creado exitosamente. PeriodId: {PeriodId}, Code: {Code}",
                periodId,
                command.Code);

            return CreatedAtAction(
                nameof(GetById),
                new { id = periodId },
                new { id = periodId, message = "Período académico creado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al crear período académico. Code: {Code}. Razón: {Reason}",
                command.Code,
                ex.Message);

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear período académico. Code: {Code}", command.Code);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al crear el período académico." });
        }
    }

    /// <summary>
    /// Actualiza los datos de un período académico existente.
    /// </summary>
    /// <param name="id">ID del período académico a actualizar.</param>
    /// <param name="command">Nuevos datos del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de actualización.</returns>
    /// <response code="200">Período académico actualizado exitosamente.</response>
    /// <response code="400">Datos inválidos o código ya en uso.</response>
    /// <response code="404">Período académico no encontrado.</response>
    /// <response code="409">El ID no coincide con el cuerpo de la petición.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Periods.Update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAcademicPeriodCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.Id != id)
                return Conflict(new { message = "El ID del período académico no coincide con el ID de la ruta." });

            await _academicPeriodsAppService.UpdateAsync(command, ct);

            _logger.LogInformation("Período académico {PeriodId} actualizado exitosamente", id);

            return Ok(new { message = "Período académico actualizado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al actualizar período académico {PeriodId}. Razón: {Reason}", id, ex.Message);

            if (ex.Message.Contains("no existe"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar período académico {PeriodId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al actualizar el período académico." });
        }
    }

    /// <summary>
    /// Activa un período académico previamente desactivado.
    /// </summary>
    /// <param name="id">ID del período académico a activar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de activación.</returns>
    /// <response code="200">Período académico activado exitosamente.</response>
    /// <response code="404">Período académico no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "Periods.Deactivate")] // ✅ mismo permiso para cambio de estado
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            await _academicPeriodsAppService.ActivateAsync(id, ct);

            _logger.LogInformation("Período académico {PeriodId} activado", id);

            return Ok(new { message = "Período académico activado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al activar período académico {PeriodId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al activar período académico {PeriodId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al activar el período académico." });
        }
    }

    /// <summary>
    /// Desactiva un período académico.
    /// </summary>
    /// <param name="id">ID del período académico a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de desactivación.</returns>
    /// <response code="200">Período académico desactivado exitosamente.</response>
    /// <response code="404">Período académico no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "Periods.Deactivate")] // ✅ mismo permiso para cambio de estado
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            await _academicPeriodsAppService.DeactivateAsync(id, ct);

            _logger.LogInformation("Período académico {PeriodId} desactivado", id);

            return Ok(new { message = "Período académico desactivado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al desactivar período académico {PeriodId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al desactivar período académico {PeriodId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al desactivar el período académico." });
        }
    }
}

using Application.Academics;
using Application.Academics.Commands;
using Application.Academics.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador para gestión de asignaturas en SIGEPP.
/// Todos los endpoints requieren autenticación con rol ADMIN.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class SubjectsController : ControllerBase
{
    private readonly SubjectsAppService _subjectsAppService;
    private readonly ILogger<SubjectsController> _logger;

    public SubjectsController(
        SubjectsAppService subjectsAppService,
        ILogger<SubjectsController> logger)
    {
        _subjectsAppService = subjectsAppService ?? throw new ArgumentNullException(nameof(subjectsAppService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene la lista de todas las asignaturas del sistema.
    /// </summary>
    /// <param name="activeOnly">Si es true, solo retorna asignaturas activas. Default: false.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaturas.</returns>
    /// <response code="200">Lista de asignaturas obtenida exitosamente.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<SubjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        try
        {
            var subjects = activeOnly
                ? await _subjectsAppService.GetActiveAsync(ct)
                : await _subjectsAppService.GetAllAsync(ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} asignaturas (activeOnly: {ActiveOnly})",
                subjects.Count,
                activeOnly);

            return Ok(subjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de asignaturas");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener lista de asignaturas." });
        }
    }

    /// <summary>
    /// Obtiene una asignatura por su ID con información detallada.
    /// </summary>
    /// <param name="id">ID de la asignatura.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Información detallada de la asignatura.</returns>
    /// <response code="200">Asignatura encontrada.</response>
    /// <response code="404">Asignatura no encontrada.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            var subject = await _subjectsAppService.GetByIdAsync(id, ct);

            if (subject == null)
            {
                _logger.LogWarning("Asignatura con ID {SubjectId} no encontrada", id);
                return NotFound(new { message = $"Asignatura con ID '{id}' no encontrada." });
            }

            return Ok(subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener asignatura {SubjectId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener la asignatura." });
        }
    }

    /// <summary>
    /// Crea una nueva asignatura en el sistema.
    /// </summary>
    /// <param name="command">Datos de la asignatura a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID de la asignatura creada.</returns>
    /// <response code="201">Asignatura creada exitosamente.</response>
    /// <response code="400">Datos inválidos o código ya en uso.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubjectCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subjectId = await _subjectsAppService.CreateAsync(command, ct);

            _logger.LogInformation(
                "Asignatura creada exitosamente. SubjectId: {SubjectId}, Code: {Code}",
                subjectId,
                command.Code);

            return CreatedAtAction(
                nameof(GetById),
                new { id = subjectId },
                new { id = subjectId, message = "Asignatura creada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al crear asignatura. Code: {Code}. Razón: {Reason}",
                command.Code,
                ex.Message);

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear asignatura. Code: {Code}", command.Code);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al crear la asignatura." });
        }
    }

    /// <summary>
    /// Actualiza los datos de una asignatura existente.
    /// </summary>
    /// <param name="id">ID de la asignatura a actualizar.</param>
    /// <param name="command">Nuevos datos de la asignatura.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de actualización.</returns>
    /// <response code="200">Asignatura actualizada exitosamente.</response>
    /// <response code="400">Datos inválidos o código ya en uso.</response>
    /// <response code="404">Asignatura no encontrada.</response>
    /// <response code="409">El ID no coincide con el cuerpo de la petición.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSubjectCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.Id != id)
                return Conflict(new { message = "El ID de la asignatura no coincide con el ID de la ruta." });

            await _subjectsAppService.UpdateAsync(command, ct);

            _logger.LogInformation("Asignatura {SubjectId} actualizada exitosamente", id);

            return Ok(new { message = "Asignatura actualizada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al actualizar asignatura {SubjectId}. Razón: {Reason}", id, ex.Message);

            if (ex.Message.Contains("no existe"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar asignatura {SubjectId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al actualizar la asignatura." });
        }
    }

    /// <summary>
    /// Activa una asignatura previamente desactivada.
    /// </summary>
    /// <param name="id">ID de la asignatura a activar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de activación.</returns>
    /// <response code="200">Asignatura activada exitosamente.</response>
    /// <response code="404">Asignatura no encontrada.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            await _subjectsAppService.ActivateAsync(id, ct);

            _logger.LogInformation("Asignatura {SubjectId} activada", id);

            return Ok(new { message = "Asignatura activada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al activar asignatura {SubjectId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al activar asignatura {SubjectId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al activar la asignatura." });
        }
    }

    /// <summary>
    /// Desactiva una asignatura.
    /// </summary>
    /// <param name="id">ID de la asignatura a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de desactivación.</returns>
    /// <response code="200">Asignatura desactivada exitosamente.</response>
    /// <response code="404">Asignatura no encontrada.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            await _subjectsAppService.DeactivateAsync(id, ct);

            _logger.LogInformation("Asignatura {SubjectId} desactivada", id);

            return Ok(new { message = "Asignatura desactivada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al desactivar asignatura {SubjectId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al desactivar asignatura {SubjectId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al desactivar la asignatura." });
        }
    }
}

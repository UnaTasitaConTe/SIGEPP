using Application.Ppa;
using Application.Ppa.Commands;
using Application.Ppa.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador para gestión de PPAs (Proyectos Pedagógicos de Aula) en SIGEPP.
/// Maneja operaciones CRUD y cambio de estado de PPAs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,DOCENTE,CONSULTA_INTERNA")]
public class PpaController : ControllerBase
{
    private readonly PpaAppService _ppaAppService;
    private readonly ILogger<PpaController> _logger;

    public PpaController(
        PpaAppService ppaAppService,
        ILogger<PpaController> logger)
    {
        _ppaAppService = ppaAppService ?? throw new ArgumentNullException(nameof(ppaAppService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene un PPA por su ID con información detallada.
    /// </summary>
    /// <param name="id">ID del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Información detallada del PPA.</returns>
    /// <response code="200">PPA encontrado.</response>
    /// <response code="404">PPA no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PpaDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            var ppa = await _ppaAppService.GetByIdAsync(id, ct);

            if (ppa == null)
            {
                _logger.LogWarning("PPA con ID {PpaId} no encontrado", id);
                return NotFound(new { message = $"PPA con ID '{id}' no encontrado." });
            }

            _logger.LogInformation("PPA {PpaId} obtenido exitosamente", id);
            return Ok(ppa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener PPA {PpaId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener el PPA." });
        }
    }

    /// <summary>
    /// Obtiene todos los PPAs de un período académico específico.
    /// </summary>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de PPAs del período.</returns>
    /// <response code="200">Lista de PPAs obtenida exitosamente.</response>
    /// <response code="400">Parámetros inválidos.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("by-period")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PpaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByAcademicPeriod(
        [FromQuery] Guid academicPeriodId,
        CancellationToken ct = default)
    {
        try
        {
            if (academicPeriodId == Guid.Empty)
                return BadRequest(new { message = "El ID del período académico es requerido." });

            var ppas = await _ppaAppService.GetByAcademicPeriodAsync(academicPeriodId, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} PPAs para el período {PeriodId}",
                ppas.Count,
                academicPeriodId);

            return Ok(ppas);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener PPAs del período {PeriodId}",
                academicPeriodId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener los PPAs del período." });
        }
    }

    /// <summary>
    /// Obtiene todos los PPAs de un docente en un período académico específico.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de PPAs del docente en el período.</returns>
    /// <response code="200">Lista de PPAs obtenida exitosamente.</response>
    /// <response code="400">Parámetros inválidos.</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// En el futuro, el teacherId podría extraerse del usuario autenticado.
    /// Por ahora se pasa como parámetro de query.
    /// </remarks>
    [HttpGet("by-teacher")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PpaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByTeacher(
        [FromQuery] Guid teacherId,
        [FromQuery] Guid academicPeriodId,
        CancellationToken ct = default)
    {
        try
        {
            if (teacherId == Guid.Empty)
                return BadRequest(new { message = "El ID del docente es requerido." });

            if (academicPeriodId == Guid.Empty)
                return BadRequest(new { message = "El ID del período académico es requerido." });

            var ppas = await _ppaAppService.GetByTeacherAsync(teacherId, academicPeriodId, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} PPAs para el docente {TeacherId} en el período {PeriodId}",
                ppas.Count,
                teacherId,
                academicPeriodId);

            return Ok(ppas);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener PPAs del docente {TeacherId} en período {PeriodId}",
                teacherId,
                academicPeriodId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener los PPAs del docente." });
        }
    }

    /// <summary>
    /// Crea un nuevo PPA en el sistema.
    /// </summary>
    /// <param name="command">Datos del PPA a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del PPA creado.</returns>
    /// <response code="201">PPA creado exitosamente.</response>
    /// <response code="400">Datos inválidos, período no activo, asignaciones inválidas, o PPA duplicado.</response>
    /// <response code="404">Período académico, docente o asignaciones no encontrados.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos (requiere rol ADMIN o DOCENTE).</response>
    [HttpPost]
    [Authorize(Roles = "ADMIN,DOCENTE")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePpaCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ppaId = await _ppaAppService.CreateAsync(command, ct);

            _logger.LogInformation(
                "PPA creado exitosamente. PpaId: {PpaId}, Título: {Title}, PeriodId: {PeriodId}, TeacherId: {TeacherId}",
                ppaId,
                command.Title,
                command.AcademicPeriodId,
                command.PrimaryTeacherId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = ppaId },
                new { id = ppaId, message = "PPA creado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al crear PPA. Título: {Title}, PeriodId: {PeriodId}. Razón: {Reason}",
                command.Title,
                command.AcademicPeriodId,
                ex.Message);

            // Determinar si es un 404 o 400 según el mensaje
            if (ex.Message.Contains("no existe") || ex.Message.Contains("no encontrado"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al crear PPA. Título: {Title}, PeriodId: {PeriodId}",
                command.Title,
                command.AcademicPeriodId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al crear el PPA." });
        }
    }

    /// <summary>
    /// Actualiza los datos de un PPA existente.
    /// </summary>
    /// <param name="id">ID del PPA a actualizar.</param>
    /// <param name="command">Nuevos datos del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de actualización.</returns>
    /// <response code="200">PPA actualizado exitosamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">PPA no encontrado.</response>
    /// <response code="409">El ID del PPA no coincide con el ID de la ruta.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos (requiere rol ADMIN o DOCENTE).</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ADMIN,DOCENTE")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePpaCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.Id != id)
                return Conflict(new { message = "El ID del PPA no coincide con el ID de la ruta." });

            await _ppaAppService.UpdateAsync(command, ct);

            _logger.LogInformation("PPA {PpaId} actualizado exitosamente", id);

            return Ok(new { message = "PPA actualizado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al actualizar PPA {PpaId}. Razón: {Reason}", id, ex.Message);

            if (ex.Message.Contains("no encontrado"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar PPA {PpaId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al actualizar el PPA." });
        }
    }

    /// <summary>
    /// Cambia el estado de un PPA (Proposal → InProgress → Completed → Archived).
    /// </summary>
    /// <param name="id">ID del PPA cuyo estado se desea cambiar.</param>
    /// <param name="command">Comando con el nuevo estado.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de cambio de estado.</returns>
    /// <response code="200">Estado cambiado exitosamente.</response>
    /// <response code="400">Cambio de estado inválido.</response>
    /// <response code="404">PPA no encontrado.</response>
    /// <response code="409">El ID del PPA no coincide con el ID de la ruta.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos (requiere rol ADMIN).</response>
    /// <remarks>
    /// NOTA: En el futuro, antes de cambiar a estado Completed, se validará que exista
    /// al menos un anexo de tipo PpaDocument. Esta validación está pendiente de implementación
    /// y se hará cuando se integren los servicios de PPA y PpaAttachments.
    /// </remarks>
    [HttpPost("{id:guid}/status")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangePpaStatusCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.Id != id)
                return Conflict(new { message = "El ID del PPA no coincide con el ID de la ruta." });

            await _ppaAppService.ChangeStatusAsync(command, ct);

            _logger.LogInformation(
                "Estado del PPA {PpaId} cambiado a {NewStatus}",
                id,
                command.NewStatus);

            return Ok(new { message = $"Estado del PPA cambiado a {command.NewStatus} exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al cambiar estado del PPA {PpaId} a {NewStatus}. Razón: {Reason}",
                id,
                command.NewStatus,
                ex.Message);

            if (ex.Message.Contains("no encontrado"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al cambiar estado del PPA {PpaId} a {NewStatus}",
                id,
                command.NewStatus);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al cambiar el estado del PPA." });
        }
    }
}

using Application.Academics;
using Application.Academics.Commands;
using Application.Academics.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador para gestión de asignaciones docentes en SIGEPP.
/// Todos los endpoints requieren autenticación con rol ADMIN.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeacherAssignmentsController : ControllerBase
{
    private readonly TeacherAssignmentsAppService _teacherAssignmentsAppService;
    private readonly ILogger<TeacherAssignmentsController> _logger;

    public TeacherAssignmentsController(
        TeacherAssignmentsAppService teacherAssignmentsAppService,
        ILogger<TeacherAssignmentsController> logger)
    {
        _teacherAssignmentsAppService = teacherAssignmentsAppService ?? throw new ArgumentNullException(nameof(teacherAssignmentsAppService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todas las asignaciones de un docente en un período académico específico.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones del docente en el período.</returns>
    /// <response code="200">Lista de asignaciones obtenida exitosamente.</response>
    /// <response code="400">Parámetros inválidos.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("by-teacher")]
    [Authorize(Policy = "TeacherSubjects.View")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TeacherAssignmentDto>), StatusCodes.Status200OK)]
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

            var assignments = await _teacherAssignmentsAppService.GetByTeacherAsync(teacherId, academicPeriodId, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} asignaciones para el docente {TeacherId} en el período {PeriodId}",
                assignments.Count,
                teacherId,
                academicPeriodId);

            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener asignaciones del docente {TeacherId} en período {PeriodId}",
                teacherId,
                academicPeriodId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener las asignaciones del docente." });
        }
    }

    /// <summary>
    /// Obtiene todas las asignaciones de una asignatura en un período académico específico.
    /// </summary>
    /// <param name="subjectId">ID de la asignatura.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones de la asignatura en el período.</returns>
    /// <response code="200">Lista de asignaciones obtenida exitosamente.</response>
    /// <response code="400">Parámetros inválidos.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("by-subject")]
    [Authorize(Policy = "TeacherSubjects.View")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TeacherAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBySubject(
        [FromQuery] Guid subjectId,
        [FromQuery] Guid academicPeriodId,
        CancellationToken ct = default)
    {
        try
        {
            if (subjectId == Guid.Empty)
                return BadRequest(new { message = "El ID de la asignatura es requerido." });

            if (academicPeriodId == Guid.Empty)
                return BadRequest(new { message = "El ID del período académico es requerido." });

            var assignments = await _teacherAssignmentsAppService.GetBySubjectAsync(subjectId, academicPeriodId, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} asignaciones para la asignatura {SubjectId} en el período {PeriodId}",
                assignments.Count,
                subjectId,
                academicPeriodId);

            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener asignaciones de la asignatura {SubjectId} en período {PeriodId}",
                subjectId,
                academicPeriodId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener las asignaciones de la asignatura." });
        }
    }

    /// <summary>
    /// Obtiene todas las asignaciones de un período académico específico.
    /// </summary>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todas las asignaciones del período.</returns>
    /// <response code="200">Lista de asignaciones obtenida exitosamente.</response>
    /// <response code="400">Parámetros inválidos.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("by-period")]
    [Authorize(Policy = "TeacherSubjects.View")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TeacherAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByPeriod(
        [FromQuery] Guid academicPeriodId,
        CancellationToken ct = default)
    {
        try
        {
            if (academicPeriodId == Guid.Empty)
                return BadRequest(new { message = "El ID del período académico es requerido." });

            var assignments = await _teacherAssignmentsAppService.GetByPeriodAsync(academicPeriodId, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} asignaciones para el período {PeriodId}",
                assignments.Count,
                academicPeriodId);

            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener asignaciones del período {PeriodId}",
                academicPeriodId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener las asignaciones del período." });
        }
    }

    /// <summary>
    /// Asigna un docente a una asignatura en un período académico.
    /// </summary>
    /// <param name="command">Datos de la asignación a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID de la asignación creada.</returns>
    /// <response code="201">Asignación creada exitosamente.</response>
    /// <response code="400">Datos inválidos o asignación duplicada.</response>
    /// <response code="404">Docente, asignatura o período académico no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost]
    [Authorize(Policy = "TeacherSubjects.Manage")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Assign(
        [FromBody] AssignTeacherCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var assignmentId = await _teacherAssignmentsAppService.AssignAsync(command, ct);

            _logger.LogInformation(
                "Asignación creada exitosamente. AssignmentId: {AssignmentId}, TeacherId: {TeacherId}, SubjectId: {SubjectId}, PeriodId: {PeriodId}",
                assignmentId,
                command.TeacherId,
                command.SubjectId,
                command.AcademicPeriodId);

            return CreatedAtAction(
                nameof(GetByPeriod),
                new { academicPeriodId = command.AcademicPeriodId },
                new { id = assignmentId, message = "Asignación creada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al crear asignación. TeacherId: {TeacherId}, SubjectId: {SubjectId}, PeriodId: {PeriodId}. Razón: {Reason}",
                command.TeacherId,
                command.SubjectId,
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
                "Error inesperado al crear asignación. TeacherId: {TeacherId}, SubjectId: {SubjectId}, PeriodId: {PeriodId}",
                command.TeacherId,
                command.SubjectId,
                command.AcademicPeriodId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al crear la asignación." });
        }
    }

    /// <summary>
    /// Desactiva una asignación docente.
    /// </summary>
    /// <param name="id">ID de la asignación a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de desactivación.</returns>
    /// <response code="200">Asignación desactivada exitosamente.</response>
    /// <response code="404">Asignación no encontrada.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "TeacherSubjects.Manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            await _teacherAssignmentsAppService.DeactivateAsync(id, ct);

            _logger.LogInformation("Asignación {AssignmentId} desactivada", id);

            return Ok(new { message = "Asignación desactivada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al desactivar asignación {AssignmentId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al desactivar asignación {AssignmentId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al desactivar la asignación." });
        }
    }

    /// <summary>
    /// Reactiva una asignación docente previamente desactivada.
    /// </summary>
    /// <param name="id">ID de la asignación a activar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de activación.</returns>
    /// <response code="200">Asignación activada exitosamente.</response>
    /// <response code="404">Asignación no encontrada.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "TeacherSubjects.Manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            await _teacherAssignmentsAppService.ActivateAsync(id, ct);

            _logger.LogInformation("Asignación {AssignmentId} activada", id);

            return Ok(new { message = "Asignación activada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al activar asignación {AssignmentId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al activar asignación {AssignmentId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al activar la asignación." });
        }
    }

    /// <summary>
    /// Elimina permanentemente una asignación docente.
    /// </summary>
    /// <param name="id">ID de la asignación a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de eliminación.</returns>
    /// <response code="200">Asignación eliminada exitosamente.</response>
    /// <response code="404">Asignación no encontrada.</response>
    /// <response code="401">No autenticado.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "TeacherSubjects.Manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            await _teacherAssignmentsAppService.DeleteAsync(id, ct);

            _logger.LogInformation("Asignación {AssignmentId} eliminada permanentemente", id);

            return Ok(new { message = "Asignación eliminada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al eliminar asignación {AssignmentId}. Razón: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar asignación {AssignmentId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al eliminar la asignación." });
        }
    }
}

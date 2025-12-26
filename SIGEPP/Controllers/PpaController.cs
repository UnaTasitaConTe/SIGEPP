using Application.Ppa;
using Application.Ppa.Commands;
using Application.Ppa.DTOs;
using Application.Security;
using Domain.Common;
using Domain.Ppa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador para gestión de PPAs (Proyectos Pedagógicos de Aula) en SIGEPP.
/// Maneja operaciones CRUD y cambio de estado de PPAs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PpaController : ControllerBase
{
    private readonly PpaAppService _ppaAppService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PpaController> _logger;

    public PpaController(
        PpaAppService ppaAppService,
        ICurrentUserService currentUserService,
        ILogger<PpaController> logger)
    {
        _ppaAppService = ppaAppService ?? throw new ArgumentNullException(nameof(ppaAppService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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
    [Authorize(Policy = "Ppa.View")] // (view_all OR view_own)
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
    [Authorize(Policy = "Ppa.View")]
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
    [Authorize(Policy = "Ppa.View")]
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
    /// Crea un nuevo PPA como administrador, especificando el docente responsable.
    /// </summary>
    /// <param name="command">Datos del PPA a crear (incluye docente responsable).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del PPA creado.</returns>
    /// <response code="201">PPA creado exitosamente.</response>
    /// <response code="400">Datos inválidos, período no activo, asignaciones inválidas, o PPA duplicado.</response>
    /// <response code="404">Período académico, docente responsable o asignaciones no encontrados.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos (requiere rol ADMIN).</response>
    [HttpPost("admin")]
    [Authorize(Policy = "Ppa.Create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsAdmin(
        [FromBody] CreatePpaAsAdminCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ppaId = await _ppaAppService.CreateAsAdminAsync(command, ct);

            _logger.LogInformation(
                "PPA creado exitosamente por admin. PpaId: {PpaId}, Título: {Title}, Responsable: {ResponsibleTeacherId}",
                ppaId,
                command.Title,
                command.ResponsibleTeacherId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = ppaId },
                new { id = ppaId, message = "PPA creado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al crear PPA como admin. Título: {Title}, Responsable: {ResponsibleTeacherId}. Razón: {Reason}",
                command.Title,
                command.ResponsibleTeacherId,
                ex.Message);

            if (ex.Message.Contains("no existe") || ex.Message.Contains("no encontrado"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al crear PPA como admin. Título: {Title}, Responsable: {ResponsibleTeacherId}",
                command.Title,
                command.ResponsibleTeacherId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al crear el PPA." });
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
    [Authorize(Policy = "Ppa.Create")]
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
                "PPA creado exitosamente. PpaId: {PpaId}, Título: {Title}, PeriodId: {PeriodId}",
                ppaId,
                command.Title,
                command.AcademicPeriodId);

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
    /// Actualiza un PPA como administrador, permitiendo cambiar el docente responsable.
    /// </summary>
    /// <param name="id">ID del PPA a actualizar.</param>
    /// <param name="command">Nuevos datos del PPA (incluye docente responsable).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de actualización.</returns>
    /// <response code="200">PPA actualizado exitosamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">PPA no encontrado.</response>
    /// <response code="409">El ID del PPA no coincide con el ID de la ruta.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos (requiere rol ADMIN).</response>
    [HttpPut("admin/{id:guid}")]
    [Authorize(Policy = "Ppa.Update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsAdmin(
        Guid id,
        [FromBody] UpdatePpaAsAdminCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.Id != id)
                return Conflict(new { message = "El ID del PPA no coincide con el ID de la ruta." });

            await _ppaAppService.UpdateAsAdminAsync(command, ct);

            _logger.LogInformation("PPA {PpaId} actualizado exitosamente por admin", id);

            return Ok(new { message = "PPA actualizado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al actualizar PPA {PpaId} como admin. Razón: {Reason}", id, ex.Message);

            if (ex.Message.Contains("no encontrado"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar PPA {PpaId} como admin", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al actualizar el PPA." });
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
    [Authorize(Policy = "Ppa.Update")]
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
    [Authorize(Policy = "Ppa.ChangeStatus")]
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

    /// <summary>
    /// Obtiene todos los PPAs del docente autenticado (como responsable o asignado).
    /// </summary>
    /// <param name="academicPeriodId">ID del período académico (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de PPAs del docente autenticado.</returns>
    /// <response code="200">Lista de PPAs obtenida exitosamente.</response>
    /// <response code="400">Parámetros inválidos.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("my")]
    [Authorize(Policy = "Ppa.View")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PpaSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPpas(
        [FromQuery] Guid? academicPeriodId = null,
        CancellationToken ct = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            var ppas = await _ppaAppService.GetPpasForTeacherAsync(currentUserId, academicPeriodId, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} PPAs para el docente autenticado {TeacherId}",
                ppas.Count,
                currentUserId);

            return Ok(ppas);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al obtener PPAs del docente autenticado. Razón: {Reason}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener PPAs del docente autenticado");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener los PPAs del docente." });
        }
    }

    /// <summary>
    /// Obtiene una lista paginada de PPAs con filtros avanzados.
    /// </summary>
    /// <param name="page">Número de página (base 1).</param>
    /// <param name="pageSize">Cantidad de elementos por página.</param>
    /// <param name="search">Texto de búsqueda en título, objetivos o descripción.</param>
    /// <param name="academicPeriodId">Filtro por período académico.</param>
    /// <param name="status">Filtro por estado del PPA.</param>
    /// <param name="responsibleTeacherId">Filtro por docente responsable.</param>
    /// <param name="teacherId">Filtro por cualquier docente vinculado (responsable o asignado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado paginado con PPAs.</returns>
    /// <response code="200">Lista paginada obtenida exitosamente.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos.</response>
    /// <remarks>
    /// Autorización basada en rol:
    /// - ADMIN: Puede ver todos los PPAs con los filtros especificados.
    /// - DOCENTE: Solo puede ver sus propios PPAs (el filtro teacherId se aplica automáticamente con su ID).
    /// </remarks>
    [HttpGet("paged")]
    [Authorize(Policy = "Ppa.View")]
    [ProducesResponseType(typeof(PagedResult<PpaSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? academicPeriodId = null,
        [FromQuery] PpaStatus? status = null,
        [FromQuery] Guid? responsibleTeacherId = null,
        [FromQuery] Guid? teacherId = null,
        CancellationToken ct = default)
    {
        try
        {
            // Obtener el usuario actual para determinar permisos
            var currentUser = _currentUserService.GetCurrentUser();
            if (currentUser == null)
            {
                _logger.LogWarning("No se pudo obtener el usuario autenticado al intentar obtener PPAs paginados");
                return Unauthorized(new { message = "No se pudo autenticar el usuario." });
            }

            // Autorización basada en rol
            var isTeacher = currentUser.Roles.Contains("DOCENTE");
             

            // Si el usuario NO es ADMIN, solo puede ver sus propios PPAs
            if (isTeacher)
            {
                teacherId = currentUser.UserId;
            }

            // Construir la consulta paginada
            var query = new PpaPagedQuery
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                AcademicPeriodId = academicPeriodId,
                Status = status,
                ResponsibleTeacherId = responsibleTeacherId,
                TeacherId = teacherId
            };

            var result = await _ppaAppService.GetPpasPagedAsync(query, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} PPAs (página {Page} de {TotalPages}). Usuario: {UserId}, Rol: {Role}",
                result.Items.Count,
                result.Page,
                result.TotalPages,
                currentUser.UserId,
                isTeacher ? "Docente" : "ADMIN/CONSULTOR");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener PPAs paginados");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener los PPAs." });
        }
    }

    /// <summary>
    /// Obtiene el historial de cambios de un PPA específico.
    /// </summary>
    /// <param name="id">ID del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de entradas de historial del PPA.</returns>
    /// <response code="200">Historial obtenido exitosamente.</response>
    /// <response code="404">PPA no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("{id:guid}/history")]
    [Authorize(Policy = "Ppa.View")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PpaHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            var history = await _ppaAppService.GetHistoryAsync(id, ct);

            _logger.LogInformation(
                "Se obtuvo historial de {Count} entradas para el PPA {PpaId}",
                history.Count,
                id);

            return Ok(history);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al obtener historial del PPA {PpaId}. Razón: {Reason}", id, ex.Message);

            if (ex.Message.Contains("no encontrado"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener historial del PPA {PpaId}", id);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener el historial del PPA." });
        }
    }

    /// <summary>
    /// Continúa un PPA existente a otro período académico.
    /// </summary>
    /// <param name="id">ID del PPA a continuar.</param>
    /// <param name="command">Datos para la continuación del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del nuevo PPA creado como continuación.</returns>
    /// <response code="201">PPA continuado exitosamente.</response>
    /// <response code="400">Datos inválidos, PPA ya continuado, o período destino inválido.</response>
    /// <response code="404">PPA origen o período académico no encontrado.</response>
    /// <response code="409">El ID del PPA no coincide con el ID de la ruta.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos (requiere ser responsable del PPA o ADMIN).</response>
    [HttpPost("{id:guid}/continue")]
    [Authorize(Policy = "Ppa.Create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ContinuePpa(
        Guid id,
        [FromBody] ContinuePpaCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.SourcePpaId != id)
                return Conflict(new { message = "El ID del PPA no coincide con el ID de la ruta." });

            var newPpaId = await _ppaAppService.ContinuePpaAsync(command, ct);

            _logger.LogInformation(
                "PPA {SourcePpaId} continuado exitosamente. Nuevo PPA: {NewPpaId}, Período destino: {TargetPeriodId}",
                id,
                newPpaId,
                command.TargetAcademicPeriodId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = newPpaId },
                new { id = newPpaId, message = "PPA continuado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al continuar PPA {SourcePpaId} al período {TargetPeriodId}. Razón: {Reason}",
                id,
                command.TargetAcademicPeriodId,
                ex.Message);

            if (ex.Message.Contains("no encontrado") || ex.Message.Contains("no existe"))
                return NotFound(new { message = ex.Message });

            if (ex.Message.Contains("No tiene permisos"))
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al continuar PPA {SourcePpaId} al período {TargetPeriodId}",
                id,
                command.TargetAcademicPeriodId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al continuar el PPA." });
        }
    }
}

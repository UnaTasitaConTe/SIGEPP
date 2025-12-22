using Application.Ppa;
using Application.Ppa.Commands;
using Application.Ppa.DTOs;
using Application.Storage;
using Domain.Ppa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador para gestión de anexos de PPA en SIGEPP.
/// Maneja operaciones de consulta, registro y eliminación lógica de archivos adjuntos a PPAs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PpaAttachmentsController : ControllerBase
{
    private readonly PpaAttachmentsAppService _ppaAttachmentsAppService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PpaAttachmentsController> _logger;

    public PpaAttachmentsController(
        PpaAttachmentsAppService ppaAttachmentsAppService,
        IFileStorageService fileStorageService,
        ILogger<PpaAttachmentsController> logger)
    {
        _ppaAttachmentsAppService = ppaAttachmentsAppService ?? throw new ArgumentNullException(nameof(ppaAttachmentsAppService));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todos los anexos de un PPA (excluyendo los eliminados).
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de anexos del PPA.</returns>
    /// <response code="200">Lista de anexos obtenida exitosamente.</response>
    /// <response code="404">PPA no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("by-ppa/{ppaId:guid}")]
    [Authorize(Policy = "Resources.View")] // ✅ view_all OR view_own (con validación de ownership)

    [ProducesResponseType(typeof(IReadOnlyCollection<PpaAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByPpa(
        Guid ppaId,
        CancellationToken ct = default)
    {
        try
        {
            var attachments = await _ppaAttachmentsAppService.GetByPpaAsync(ppaId, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} anexos para el PPA {PpaId}",
                attachments.Count,
                ppaId);

            return Ok(attachments);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al obtener anexos del PPA {PpaId}. Razón: {Reason}",
                ppaId,
                ex.Message);

            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener anexos del PPA {PpaId}", ppaId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener los anexos del PPA." });
        }
    }

    /// <summary>
    /// Obtiene todos los anexos de un PPA filtrados por tipo.
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="type">Tipo de anexo (PpaDocument, TeacherAuthorization, Evidence, etc.).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de anexos del PPA del tipo especificado.</returns>
    /// <response code="200">Lista de anexos obtenida exitosamente.</response>
    /// <response code="400">Parámetros inválidos.</response>
    /// <response code="404">PPA no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet("by-ppa-and-type")]
    [Authorize(Policy = "Resources.View")] // ✅ view_all OR view_own (con validación de ownership)

    [ProducesResponseType(typeof(IReadOnlyCollection<PpaAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByPpaAndType(
        [FromQuery] Guid ppaId,
        [FromQuery] PpaAttachmentType type,
        CancellationToken ct = default)
    {
        try
        {
            if (ppaId == Guid.Empty)
                return BadRequest(new { message = "El ID del PPA es requerido." });

            var attachments = await _ppaAttachmentsAppService.GetByPpaAndTypeAsync(ppaId, type, ct);

            _logger.LogInformation(
                "Se obtuvieron {Count} anexos de tipo {Type} para el PPA {PpaId}",
                attachments.Count,
                type,
                ppaId);

            return Ok(attachments);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al obtener anexos del PPA {PpaId} de tipo {Type}. Razón: {Reason}",
                ppaId,
                type,
                ex.Message);

            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener anexos del PPA {PpaId} de tipo {Type}",
                ppaId,
                type);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener los anexos del PPA." });
        }
    }

    /// <summary>
    /// Registra la metadata de un nuevo anexo en un PPA.
    /// NOTA: Este endpoint solo registra la información del anexo (metadata).
    /// La subida física del archivo al almacenamiento se debe manejar en otro módulo/servicio.
    /// </summary>
    /// <param name="ppaId">ID del PPA al que se agregará el anexo.</param>
    /// <param name="request">Datos del anexo a registrar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del anexo registrado.</returns>
    /// <response code="201">Anexo registrado exitosamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">PPA o usuario no encontrado.</response>
    /// <response code="401">No autenticado o token sin userId.</response>
    /// <response code="403">No tiene permisos (requiere rol ADMIN o DOCENTE).</response>
    [HttpPost("{ppaId:guid}")]
    [Authorize(Policy = "Resources.Create")] // ✅
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Add(
        Guid ppaId,
        [FromBody] AddPpaAttachmentRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extraer userId del token JWT
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Intento de agregar anexo sin userId válido en el token");
                return Unauthorized(new { message = "Token inválido o userId no encontrado." });
            }

            // Construir el comando
            var command = new AddPpaAttachmentCommand
            {
                PpaId = ppaId,
                Type = request.Type,
                Name = request.Name,
                FileKey = request.FileKey,
                ContentType = request.ContentType,
                UploadedByUserId = userId
            };

            var attachmentId = await _ppaAttachmentsAppService.AddAsync(command, ct);

            _logger.LogInformation(
                "Anexo registrado exitosamente. AttachmentId: {AttachmentId}, PpaId: {PpaId}, Type: {Type}, UserId: {UserId}",
                attachmentId,
                ppaId,
                request.Type,
                userId);

            return CreatedAtAction(
                nameof(GetByPpa),
                new { ppaId },
                new { id = attachmentId, message = "Anexo registrado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al registrar anexo para PPA {PpaId}. Razón: {Reason}",
                ppaId,
                ex.Message);

            if (ex.Message.Contains("no existe") || ex.Message.Contains("no encontrado"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al registrar anexo para PPA {PpaId}",
                ppaId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al registrar el anexo." });
        }
    }

    /// <summary>
    /// Elimina lógicamente un anexo de PPA (soft delete).
    /// El anexo se marca como eliminado pero no se elimina físicamente de la base de datos.
    /// </summary>
    /// <param name="attachmentId">ID del anexo a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Confirmación de eliminación.</returns>
    /// <response code="200">Anexo eliminado exitosamente.</response>
    /// <response code="404">Anexo no encontrado.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos (requiere rol ADMIN o DOCENTE).</response>
    /// <remarks>
    /// NOTA: En el futuro, se validará que no se pueda eliminar el último anexo de tipo
    /// PpaDocument si el PPA está en estado Completed.
    /// </remarks>
    [HttpDelete("{attachmentId:guid}")]
    [Authorize(Policy = "Resources.Delete")] // ✅
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(
        Guid attachmentId,
        CancellationToken ct = default)
    {
        try
        {
            await _ppaAttachmentsAppService.DeleteAsync(attachmentId, ct);

            _logger.LogInformation("Anexo {AttachmentId} eliminado (soft delete)", attachmentId);

            return Ok(new { message = "Anexo eliminado (soft delete) exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Error al eliminar anexo {AttachmentId}. Razón: {Reason}",
                attachmentId,
                ex.Message);

            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar anexo {AttachmentId}", attachmentId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al eliminar el anexo." });
        }
    }

    /// <summary>
    /// Descarga un anexo de PPA.
    /// Valida que el anexo exista y no esté eliminado antes de descargarlo.
    /// </summary>
    /// <param name="attachmentId">ID del anexo a descargar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Stream del archivo con el tipo de contenido apropiado.</returns>
    /// <response code="200">Archivo descargado exitosamente.</response>
    /// <response code="404">Anexo no encontrado o eliminado.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="500">Error al descargar el archivo.</response>
    /// <remarks>
    /// Este endpoint descarga un archivo de anexo de PPA.
    /// Valida que el anexo exista en la base de datos y no esté marcado como eliminado
    /// antes de descargar el archivo desde el almacenamiento físico (MinIO).
    /// </remarks>
    [HttpGet("download/{attachmentId:guid}")]
    [Authorize(Policy = "Resources.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadAttachment(
        Guid attachmentId,
        CancellationToken ct = default)
    {
        try
        {
            // Obtener el anexo para validar que existe y obtener el fileKey
            var attachment = await _ppaAttachmentsAppService.GetByIdAsync(attachmentId, ct);

            _logger.LogInformation(
                "Iniciando descarga de anexo. AttachmentId: {AttachmentId}, FileKey: {FileKey}, Name: {Name}",
                attachmentId,
                attachment.FileKey,
                attachment.Name);

            // Obtener el archivo del almacenamiento
            var stream = await _fileStorageService.GetAsync(attachment.FileKey, ct);

            // Determinar el content type
            var contentType = !string.IsNullOrWhiteSpace(attachment.ContentType)
                ? attachment.ContentType
                : GetContentType(attachment.Name);

            // Sanitizar el nombre del archivo para evitar problemas
            var fileName = SanitizeFileName(attachment.Name);

            _logger.LogInformation(
                "Anexo descargado exitosamente. AttachmentId: {AttachmentId}, FileName: {FileName}, ContentType: {ContentType}",
                attachmentId,
                fileName,
                contentType);

            // Retornar el archivo como respuesta
            return File(stream, contentType, fileName, enableRangeProcessing: true);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Anexo no encontrado o eliminado. AttachmentId: {AttachmentId}",
                attachmentId);

            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al descargar anexo. AttachmentId: {AttachmentId}",
                attachmentId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al descargar el anexo." });
        }
    }

    /// <summary>
    /// Determina el content type basado en la extensión del archivo.
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Sanitiza el nombre del archivo para evitar problemas en la descarga.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "archivo";

        // Eliminar caracteres no válidos en nombres de archivo
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(sanitized) ? "archivo" : sanitized;
    }
}

/// <summary>
/// Request para agregar un anexo a un PPA.
/// NOTA: Este DTO solo contiene la metadata del anexo.
/// La subida física del archivo se debe manejar en otro módulo/servicio.
/// </summary>
public sealed record AddPpaAttachmentRequest
{
    /// <summary>
    /// Tipo de anexo (documento PPA, autorización, evidencia, etc.).
    /// </summary>
    [Required(ErrorMessage = "El tipo de anexo es requerido.")]
    public required PpaAttachmentType Type { get; init; }

    /// <summary>
    /// Nombre amigable del anexo para mostrar al usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre del anexo es requerido.")]
    [StringLength(300, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 300 caracteres.")]
    public required string Name { get; init; }

    /// <summary>
    /// Clave o ruta lógica del archivo en el sistema de almacenamiento.
    /// Este valor lo genera el módulo de almacenamiento de archivos.
    /// </summary>
    [Required(ErrorMessage = "La clave del archivo es requerida.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "La clave del archivo debe tener entre 1 y 500 caracteres.")]
    public required string FileKey { get; init; }

    /// <summary>
    /// Tipo MIME del archivo (opcional).
    /// Ejemplo: "application/pdf", "image/png", "application/zip".
    /// </summary>
    [StringLength(100, ErrorMessage = "El tipo de contenido no puede exceder los 100 caracteres.")]
    public string? ContentType { get; init; }
}

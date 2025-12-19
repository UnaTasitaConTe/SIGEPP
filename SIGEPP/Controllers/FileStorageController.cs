using Application.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SIGEPP.Controllers;

/// <summary>
/// Controlador para gestión de almacenamiento de archivos en SIGEPP.
/// Proporciona endpoints para subir archivos a MinIO y obtener fileKeys.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Ppa.UploadFile")]
public sealed class FileStorageController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileStorageController> _logger;

    public FileStorageController(
        IFileStorageService fileStorageService,
        ILogger<FileStorageController> logger)
    {
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sube un archivo al almacenamiento (MinIO) y devuelve el fileKey generado.
    /// El fileKey puede usarse posteriormente para registrar anexos de PPA en la base de datos.
    /// </summary>
    /// <param name="request">Datos del archivo a subir (multipart/form-data).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>FileKey y metadatos básicos del archivo.</returns>
    /// <response code="200">Archivo subido exitosamente. Retorna el fileKey generado.</response>
    /// <response code="400">Archivo vacío o datos inválidos.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos (requiere rol ADMIN o DOCENTE).</response>
    /// <response code="500">Error al subir el archivo al almacenamiento.</response>
    /// <remarks>
    /// Este endpoint solo sube el archivo físico a MinIO y retorna el fileKey.
    /// Para registrar el archivo como anexo de un PPA, se debe usar posteriormente
    /// el endpoint POST /api/PpaAttachments/{ppaId} con el fileKey obtenido aquí.
    ///
    /// El request debe enviarse como multipart/form-data con los siguientes campos:
    /// - File: archivo a subir (requerido)
    /// - Folder: carpeta lógica donde se almacenará (opcional, default: "uploads")
    ///
    /// Ejemplo de uso con cURL:
    /// curl -X POST "https://localhost:5001/api/FileStorage/upload" \
    ///   -H "Authorization: Bearer {token}" \
    ///   -F "File=@documento.pdf" \
    ///   -F "Folder=ppa/123e4567-e89b-12d3-a456-426614174000"
    /// </remarks>
    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)] // Límite: 50 MB
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Upload(
        [FromForm] UploadFileRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.File is null || request.File.Length == 0)
                return BadRequest(new { message = "El archivo está vacío o no se proporcionó." });

            // Determinar la carpeta donde se almacenará
            var folder = string.IsNullOrWhiteSpace(request.Folder)
                ? "uploads"
                : request.Folder.Trim();

            // Extraer userId del token JWT (opcional, solo para logging)
            Guid? userId = null;
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            _logger.LogInformation(
                "Iniciando subida de archivo. FileName: {FileName}, ContentType: {ContentType}, Size: {Size}, Folder: {Folder}, UserId: {UserId}",
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                folder,
                userId);

            // Abrir stream del archivo
            await using var stream = request.File.OpenReadStream();

            // Subir el archivo al almacenamiento y obtener el fileKey
            var fileKey = await _fileStorageService.UploadAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                folder,
                ct);

            _logger.LogInformation(
                "Archivo subido correctamente. FileKey: {FileKey}, OriginalFileName: {FileName}, Folder: {Folder}, UserId: {UserId}",
                fileKey,
                request.File.FileName,
                folder,
                userId);

            return Ok(new
            {
                fileKey,
                originalFileName = request.File.FileName,
                contentType = request.File.ContentType,
                size = request.File.Length,
                folder,
                message = "Archivo subido exitosamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Error al subir archivo. FileName: {FileName}",
                request.File?.FileName);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al subir archivo. FileName: {FileName}",
                request.File?.FileName);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error inesperado al subir el archivo." });
        }
    }
}

/// <summary>
/// Request para subir un archivo al almacenamiento.
/// Se envía como multipart/form-data.
/// </summary>
public sealed class UploadFileRequest
{
    /// <summary>
    /// Archivo a subir (multipart/form-data).
    /// </summary>
    [Required(ErrorMessage = "El archivo es requerido.")]
    public required IFormFile File { get; init; }

    /// <summary>
    /// Carpeta lógica o prefijo donde se almacenará el archivo.
    /// Ejemplo: "ppa/{ppaId}" o "evidencias/{userId}".
    /// Si no se envía, se usará "uploads" como default.
    /// </summary>
    [StringLength(200, ErrorMessage = "La carpeta no puede exceder los 200 caracteres.")]
    public string? Folder { get; init; }
}

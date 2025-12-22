namespace Application.Storage;

/// <summary>
/// Servicio de almacenamiento de archivos.
/// Abstrae el almacenamiento físico permitiendo diferentes implementaciones (MinIO, Azure Blob, S3, etc.).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Sube un archivo al almacenamiento y retorna una clave lógica (fileKey) para referenciarlo después.
    /// </summary>
    /// <param name="stream">Contenido del archivo.</param>
    /// <param name="fileName">Nombre original del archivo.</param>
    /// <param name="contentType">MIME type (ej: application/pdf).</param>
    /// <param name="folder">Carpeta lógica o prefijo (ej: "ppa/{ppaId}").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>FileKey que luego se almacenará en BD o se usará como referencia.</returns>
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string? contentType,
        string folder,
        CancellationToken ct = default);

    /// <summary>
    /// Elimina un archivo del almacenamiento físico identificándolo por su fileKey.
    /// </summary>
    /// <param name="fileKey">Clave del archivo a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task DeleteAsync(string fileKey, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un archivo del almacenamiento como Stream para descargarlo.
    /// </summary>
    /// <param name="fileKey">Clave del archivo a obtener.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Stream con el contenido del archivo.</returns>
    Task<Stream> GetAsync(string fileKey, CancellationToken ct = default);
}

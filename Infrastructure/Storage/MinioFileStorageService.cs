using System.Text.RegularExpressions;
using Application.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Storage;

/// <summary>
/// Implementación de IFileStorageService usando MinIO como backend de almacenamiento.
/// </summary>
public sealed class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;
    private readonly ILogger<MinioFileStorageService> _logger;
    private bool _bucketEnsured;

    public MinioFileStorageService(
        IMinioClient minioClient,
        IOptions<MinioOptions> options,
        ILogger<MinioFileStorageService> logger)
    {
        _minioClient = minioClient ?? throw new ArgumentNullException(nameof(minioClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sube un archivo a MinIO y retorna el fileKey (objectName en MinIO).
    /// </summary>
    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string? contentType,
        string folder,
        CancellationToken ct = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("El nombre de archivo es requerido.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(folder))
            folder = "uploads";

        try
        {
            // Asegurar que el bucket existe (lazy initialization)
            await EnsureBucketExistsAsync(ct).ConfigureAwait(false);

            // Sanitizar el nombre del archivo para evitar problemas con caracteres especiales
            var sanitizedFileName = SanitizeFileName(fileName);

            // Generar el objectName (fileKey) con formato: folder/guid_nombrearchivo
            var objectName = $"{folder.TrimEnd('/')}/{Guid.NewGuid():N}_{sanitizedFileName}";

            // Preparar argumentos para subir el archivo
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            // Subir el archivo a MinIO
            await _minioClient.PutObjectAsync(putObjectArgs, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Archivo subido correctamente a MinIO. Bucket: {Bucket}, ObjectName: {ObjectName}, ContentType: {ContentType}, Size: {Size}",
                _options.BucketName,
                objectName,
                contentType,
                stream.Length);

            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al subir archivo a MinIO. Bucket: {Bucket}, FileName: {FileName}, Folder: {Folder}",
                _options.BucketName,
                fileName,
                folder);

            throw new InvalidOperationException("Error al subir el archivo al almacenamiento.", ex);
        }
    }

    /// <summary>
    /// Elimina un archivo de MinIO usando su fileKey.
    /// </summary>
    public async Task DeleteAsync(string fileKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
            throw new ArgumentException("El fileKey es requerido.", nameof(fileKey));

        try
        {
            await EnsureBucketExistsAsync(ct).ConfigureAwait(false);

            var removeArgs = new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileKey);

            await _minioClient.RemoveObjectAsync(removeArgs, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Archivo eliminado de MinIO. Bucket: {Bucket}, ObjectName: {ObjectName}",
                _options.BucketName,
                fileKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al eliminar archivo de MinIO. Bucket: {Bucket}, ObjectName: {ObjectName}",
                _options.BucketName,
                fileKey);

            throw new InvalidOperationException("Error al eliminar el archivo del almacenamiento.", ex);
        }
    }

    /// <summary>
    /// Obtiene un archivo de MinIO y retorna un Stream para su descarga.
    /// </summary>
    public async Task<Stream> GetAsync(string fileKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
            throw new ArgumentException("El fileKey es requerido.", nameof(fileKey));

        try
        {
            await EnsureBucketExistsAsync(ct).ConfigureAwait(false);

            var memoryStream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileKey)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(getObjectArgs, ct).ConfigureAwait(false);

            // Resetear la posición del stream para que pueda leerse desde el inicio
            memoryStream.Position = 0;

            _logger.LogInformation(
                "Archivo obtenido de MinIO. Bucket: {Bucket}, ObjectName: {ObjectName}, Size: {Size}",
                _options.BucketName,
                fileKey,
                memoryStream.Length);

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener archivo de MinIO. Bucket: {Bucket}, ObjectName: {ObjectName}",
                _options.BucketName,
                fileKey);

            throw new InvalidOperationException("Error al obtener el archivo del almacenamiento.", ex);
        }
    }

    /// <summary>
    /// Asegura que el bucket configurado exista en MinIO.
    /// Si no existe, lo crea automáticamente.
    /// </summary>
    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        if (_bucketEnsured)
            return;

        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.BucketName),
            ct).ConfigureAwait(false);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_options.BucketName),
                ct).ConfigureAwait(false);

            _logger.LogInformation("Bucket {Bucket} creado en MinIO.", _options.BucketName);
        }

        _bucketEnsured = true;
    }

    /// <summary>
    /// Sanitiza el nombre del archivo para que sea compatible con el almacenamiento.
    /// Reemplaza espacios por guiones bajos y elimina caracteres especiales.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var name = fileName.Trim();

        // Reemplazar espacios por guiones bajos
        name = Regex.Replace(name, @"\s+", "_");

        // Eliminar caracteres no alfanuméricos excepto puntos, guiones y guiones bajos
        name = Regex.Replace(name, @"[^a-zA-Z0-9\.\-_]", "_");

        return name;
    }
}

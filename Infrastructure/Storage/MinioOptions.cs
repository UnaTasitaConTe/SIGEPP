namespace Infrastructure.Storage;

/// <summary>
/// Opciones de configuración para MinIO.
/// Se carga desde la sección "Minio" en appsettings.json.
/// </summary>
public sealed class MinioOptions
{
    /// <summary>
    /// Nombre de la sección en appsettings.json.
    /// </summary>
    public const string SectionName = "Minio";

    /// <summary>
    /// URL del servidor MinIO (sin puerto).
    /// Ejemplo: "localhost" o "minio.example.com".
    /// </summary>
    public string Endpoint { get; init; } = null!;

    /// <summary>
    /// Puerto del servidor MinIO.
    /// Ejemplo: 9000.
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Indica si se debe usar SSL/TLS.
    /// </summary>
    public bool UseSsl { get; init; }

    /// <summary>
    /// Access Key de MinIO (equivalente a username).
    /// </summary>
    public string AccessKey { get; init; } = null!;

    /// <summary>
    /// Secret Key de MinIO (equivalente a password).
    /// </summary>
    public string SecretKey { get; init; } = null!;

    /// <summary>
    /// Nombre del bucket donde se almacenarán los archivos.
    /// Ejemplo: "sigepp-files".
    /// </summary>
    public string BucketName { get; init; } = null!;
}

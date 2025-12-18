using Application.Security;
using Application.Storage;
using Domain.Academics.Repositories;
using Domain.Ppa.Repositories;
using Domain.Security.Repositories;
using Domain.Users;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Repositories.Academics;
using Infrastructure.Persistence.Repositories.Ppa;
using Infrastructure.Security;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Minio;

namespace Infrastructure;

/// <summary>
/// Métodos de extensión para registrar servicios de Infrastructure.
/// Sigue el principio de Single Responsibility (SOLID) separando
/// la configuración de dependencias del Program.cs.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios de Infrastructure (DbContext, Repositories, etc.)
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Registrar DbContext
        services.AddDatabase(configuration, environment);

        // Registrar Repositories
        services.AddRepositories();

        // Registrar servicios de seguridad
        services.AddSecurity(configuration);

        // Registrar servicios de almacenamiento
        services.AddStorage(configuration);

        return services;
    }

    /// <summary>
    /// Registra el DbContext con configuración de SQL Server
    /// </summary>
    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' no encontrado. " +
                    "Configura la cadena de conexión en appsettings.json o appsettings.Development.json");
            }

            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    // Assembly donde están las migraciones
                    npgsqlOptions.MigrationsAssembly(typeof(DependencyInjection).Assembly.GetName().Name);

                    // Timeouts y retry policy
                    npgsqlOptions.CommandTimeout(60);
                  
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });

            // Logging detallado solo en desarrollo
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    /// <summary>
    /// Registra todos los repositorios del sistema
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Repositorios de seguridad
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // Repositorios de usuarios
        services.AddScoped<IUserRepository, UserRepository>();

        // Repositorios de módulo académico
        services.AddScoped<IAcademicPeriodRepository, AcademicPeriodRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ITeacherAssignmentRepository, TeacherAssignmentRepository>();

        // Repositorios de PPA
        services.AddScoped<IPpaRepository, PpaRepository>();
        services.AddScoped<IPpaAttachmentRepository, PpaAttachmentRepository>();

        return services;
    }

    /// <summary>
    /// Registra servicios de seguridad (JWT, Password Hashing).
    /// </summary>
    private static IServiceCollection AddSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Por esta línea correcta:
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Validar opciones al iniciar la aplicación 
        services.AddOptions<JwtOptions>()
            .Validate(options =>
            {
                try
                {
                    options.Validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            }, "Configuración de JWT inválida. Verifica appsettings.json");

        // Registrar servicios de seguridad
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    /// <summary>
    /// Registra servicios de almacenamiento de archivos (MinIO).
    /// </summary>
    private static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar opciones de MinIO desde appsettings
        services.Configure<MinioOptions>(configuration.GetSection(MinioOptions.SectionName));

        // Registrar MinioClient como singleton
        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;

            var clientBuilder = new MinioClient()
                .WithEndpoint(options.Endpoint, options.Port)
                .WithCredentials(options.AccessKey, options.SecretKey);

            if (options.UseSsl)
            {
                clientBuilder = clientBuilder.WithSSL();
            }

            return clientBuilder.Build();
        });

        // Registrar servicio de almacenamiento
        services.AddSingleton<IFileStorageService, MinioFileStorageService>();

        return services;
    }
}

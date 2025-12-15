using Domain.Security.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        return services;
    }
}

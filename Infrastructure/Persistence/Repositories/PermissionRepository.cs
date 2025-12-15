using Domain.Security.Repositories;
using Domain.Security.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación concreta del repositorio de permisos.
/// Maneja la recuperación de permisos desde la base de datos.
/// Los permisos son principalmente de solo lectura (seed inicial).
/// </summary>
public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var permissionEntity = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

        return permissionEntity != null
            ? Permission.Create(permissionEntity.Code)
            : null;
    }

    public async Task<IReadOnlyCollection<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var permissionEntities = await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);

        return permissionEntities
            .Select(p => Permission.Create(p.Code))
            .ToList();
    }

    public async Task<IReadOnlyCollection<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default)
    {
        var permissionEntities = await _context.Permissions
            .Where(p => p.Module == module.ToLower())
            .OrderBy(p => p.Action)
            .ToListAsync(cancellationToken);

        return permissionEntities
            .Select(p => Permission.Create(p.Code))
            .ToList();
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AnyAsync(p => p.Code == code, cancellationToken);
    }
}

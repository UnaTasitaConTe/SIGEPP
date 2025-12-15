using Domain.Security.Entities;
using Domain.Security.Repositories;
using Domain.Security.ValueObjects;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación concreta del repositorio de roles.
/// Maneja la persistencia y recuperación de roles desde la base de datos.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var roleEntity = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);

        return roleEntity != null ? MapToDomain(roleEntity) : null;
    }

    public async Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var roleEntity = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return roleEntity != null ? MapToDomain(roleEntity) : null;
    }

    public async Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roleEntities = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);

        return [.. roleEntities.Select(MapToDomain)];
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AnyAsync(r => r.Code == code, cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        var roleEntity = MapToEntity(role);
        await _context.Roles.AddAsync(roleEntity, cancellationToken);
    }

    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        var roleEntity = MapToEntity(role);
        _context.Roles.Update(roleEntity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        if (role.IsSystemRole)
            throw new InvalidOperationException("No se puede eliminar un rol del sistema");

        var roleEntity = new RoleEntity { Id = role.Id };
        _context.Roles.Remove(roleEntity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Mapea una RoleEntity de EF Core a una Role de dominio
    /// </summary>
    private static Role MapToDomain(RoleEntity entity)
    {
        var permissions = entity.RolePermissions
            .Select(rp => Permission.Create(rp.Permission.Code))
            .ToArray();

        return Role.Reconstruct(
            id: entity.Id,
            code: entity.Code,
            name: entity.Name,
            description: entity.Description,
            isSystemRole: entity.IsSystemRole,
            permissions: permissions
        );
    }

    /// <summary>
    /// Mapea una Role de dominio a una RoleEntity de EF Core
    /// </summary>
    private static RoleEntity MapToEntity(Role role)
    {
        return new RoleEntity
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole
        };
    }
}

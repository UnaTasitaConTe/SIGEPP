using System.Reflection;
using Domain.Security.Entities;
using Domain.Security.ValueObjects;
using Domain.Users;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de usuarios usando EF Core.
/// Maneja el mapeo entre entidades de dominio (User, Role) y entidades de persistencia (UserEntity, RoleEntity).
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Obtiene un usuario por su ID, incluyendo sus roles.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var userEntity = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return userEntity == null ? null : MapToDomain(userEntity);
    }

    /// <summary>
    /// Obtiene un usuario por su email, incluyendo sus roles.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var userEntity = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        return userEntity == null ? null : MapToDomain(userEntity);
    }

    /// <summary>
    /// Verifica si existe un usuario con el email especificado.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
    }

    /// <summary>
    /// Verifica si existe un usuario con el email especificado, excluyendo un ID específico.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, Guid excludeUserId, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users.AnyAsync(
            u => u.Email == normalizedEmail && u.Id != excludeUserId,
            ct);
    }

    /// <summary>
    /// Agrega un nuevo usuario al repositorio.
    /// </summary>
    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Crear la entidad de usuario
        var userEntity = new UserEntity
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        // Agregar relaciones con roles
        foreach (var role in user.Roles)
        {
            userEntity.UserRoles.Add(new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }

        await _context.Users.AddAsync(userEntity, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Cargar la entidad existente con sus relaciones
        var userEntity = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == user.Id, ct);

        if (userEntity == null)
            throw new InvalidOperationException($"Usuario con ID '{user.Id}' no encontrado en la base de datos.");

        // Actualizar propiedades básicas
        userEntity.Name = user.Name;
        userEntity.Email = user.Email;
        userEntity.PasswordHash = user.PasswordHash;
        userEntity.IsActive = user.IsActive;
        userEntity.UpdatedAt = user.UpdatedAt;

        // Sincronizar roles
        // 1. Obtener roles actuales del dominio
        var currentRoleIds = user.Roles.Select(r => r.Id).ToHashSet();

        // 2. Obtener roles actuales en la BD
        var existingRoleIds = userEntity.UserRoles.Select(ur => ur.RoleId).ToHashSet();

        // 3. Eliminar roles que ya no están en el dominio
        var rolesToRemove = userEntity.UserRoles
            .Where(ur => !currentRoleIds.Contains(ur.RoleId))
            .ToList();

        foreach (var userRole in rolesToRemove)
        {
            userEntity.UserRoles.Remove(userRole);
            _context.UserRoles.Remove(userRole);
        }

        // 4. Agregar roles nuevos que están en el dominio pero no en la BD
        var rolesToAdd = currentRoleIds
            .Where(roleId => !existingRoleIds.Contains(roleId))
            .ToList();

        foreach (var roleId in rolesToAdd)
        {
            userEntity.UserRoles.Add(new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = roleId
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Obtiene todos los usuarios activos del sistema.
    /// </summary>
    public async Task<IReadOnlyCollection<User>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var userEntities = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync(ct);

        return userEntities.Select(MapToDomain).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todos los usuarios del sistema (activos e inactivos).
    /// </summary>
    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken ct = default)
    {
        var userEntities = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .OrderBy(u => u.Name)
            .ToListAsync(ct);

        return userEntities.Select(MapToDomain).ToList().AsReadOnly();
    }

    /// <summary>
    /// Mapea una entidad de EF Core (UserEntity) a una entidad de dominio (User).
    /// </summary>
    private User MapToDomain(UserEntity userEntity)
    {
        // Crear el usuario usando el método de fábrica con ID específico
        var user = User.CreateWithId(
            id: userEntity.Id,
            name: userEntity.Name,
            email: userEntity.Email,
            passwordHash: userEntity.PasswordHash,
            isActive: userEntity.IsActive);

        // Recrear las fechas usando reflection (propiedades con private set)
        var createdAtProperty = typeof(User).GetProperty(nameof(User.CreatedAt), BindingFlags.Public | BindingFlags.Instance);
        var updatedAtProperty = typeof(User).GetProperty(nameof(User.UpdatedAt), BindingFlags.Public | BindingFlags.Instance);

        if (createdAtProperty != null && createdAtProperty.CanWrite)
        {
            createdAtProperty.SetValue(user, userEntity.CreatedAt);
        }

        if (updatedAtProperty != null && updatedAtProperty.CanWrite)
        {
            updatedAtProperty.SetValue(user, userEntity.UpdatedAt);
        }

        // Mapear roles
        foreach (var userRole in userEntity.UserRoles)
        {
            var roleEntity = userRole.Role;

            // Mapear permisos del rol
            var permissions = roleEntity.RolePermissions
                .Select(rp => Permission.Create($"{rp.Permission.Module}.{rp.Permission.Action}"))
                .ToList();

            // Reconstruir el rol de dominio desde persistencia
            var role = Role.Reconstruct(
                id: roleEntity.Id,
                code: roleEntity.Code,
                name: roleEntity.Name,
                description: roleEntity.Description,
                isSystemRole: roleEntity.IsSystemRole,
                permissions: permissions);

            // Asignar el rol al usuario
            user.AssignRole(role);
        }

        return user;
    }
}

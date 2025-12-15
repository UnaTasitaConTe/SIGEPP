using Domain.Security.ValueObjects;

namespace Domain.Security.Entities;

/// <summary>
/// Entity que representa un rol en el sistema.
/// Un rol tiene identidad propia y agrupa un conjunto de permisos.
/// Sigue el patrón de diseño RBAC (Role-Based Access Control).
/// Esta entity es persistible y se mapea a la tabla Roles.
/// </summary>
public sealed class Role : IEquatable<Role>
{
    private readonly HashSet<Permission> _permissions;

    /// <summary>
    /// Identificador único del rol (PK en BD)
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    /// Código único del rol (ej: "ADMIN", "DOCENTE", "CONSULTA_INTERNA")
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Nombre descriptivo del rol
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Descripción del propósito y alcance del rol
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Indica si es un rol del sistema (no puede ser eliminado)
    /// </summary>
    public bool IsSystemRole { get; private set; }

    /// <summary>
    /// Colección de permisos asignados al rol (solo lectura)
    /// </summary>
    public IReadOnlySet<Permission> Permissions => _permissions;

    // Constructor privado para EF Core
    private Role()
    {
        Code = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        _permissions = new HashSet<Permission>();
    }

    private Role(long id, string code, string name, string description, bool isSystemRole, IEnumerable<Permission> permissions)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código del rol no puede estar vacío", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(name));

        Id = id;
        Code = code;
        Name = name;
        Description = description ?? string.Empty;
        IsSystemRole = isSystemRole;
        _permissions = new HashSet<Permission>(permissions ?? Enumerable.Empty<Permission>());
    }

    /// <summary>
    /// Crea un nuevo rol (para casos de uso en memoria, antes de persistir)
    /// </summary>
    public static Role Create(string code, string name, string description, bool isSystemRole, params Permission[] permissions)
    {
        return new Role(0, code, name, description, isSystemRole, permissions);
    }

    /// <summary>
    /// Crea un nuevo rol con los permisos especificados a partir de strings
    /// </summary>
    public static Role Create(string code, string name, string description, bool isSystemRole, params string[] permissionValues)
    {
        var permissions = permissionValues.Select(Permission.Create).ToArray();
        return new Role(0, code, name, description, isSystemRole, permissions);
    }

    /// <summary>
    /// Reconstruye un rol desde la BD (usado por repositorios)
    /// </summary>
    public static Role Reconstruct(long id, string code, string name, string description, bool isSystemRole, IEnumerable<Permission> permissions)
    {
        return new Role(id, code, name, description, isSystemRole, permissions);
    }

    /// <summary>
    /// Verifica si el rol tiene un permiso específico
    /// </summary>
    public bool HasPermission(Permission permission)
    {
        return _permissions.Contains(permission);
    }

    /// <summary>
    /// Verifica si el rol tiene un permiso específico por su valor string
    /// </summary>
    public bool HasPermission(string permissionValue)
    {
        return _permissions.Any(p => p.Value == permissionValue.ToLowerInvariant());
    }

    /// <summary>
    /// Verifica si el rol tiene todos los permisos especificados
    /// </summary>
    public bool HasAllPermissions(params Permission[] permissions)
    {
        return permissions.All(p => _permissions.Contains(p));
    }

    /// <summary>
    /// Verifica si el rol tiene al menos uno de los permisos especificados
    /// </summary>
    public bool HasAnyPermission(params Permission[] permissions)
    {
        return permissions.Any(p => _permissions.Contains(p));
    }

    /// <summary>
    /// Agrega un permiso al rol (usado por repositorios al cargar desde BD)
    /// </summary>
    internal void AddPermission(Permission permission)
    {
        _permissions.Add(permission);
    }

    /// <summary>
    /// Remueve un permiso del rol
    /// </summary>
    internal void RemovePermission(Permission permission)
    {
        _permissions.Remove(permission);
    }

    /// <summary>
    /// Limpia todos los permisos
    /// </summary>
    internal void ClearPermissions()
    {
        _permissions.Clear();
    }

    public override string ToString() => $"{Name} ({Code})";

    public override bool Equals(object? obj)
    {
        return obj is Role other && Equals(other);
    }

    public bool Equals(Role? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        // La igualdad se basa en el ID si ambos están persistidos, sino en el Code
        if (Id > 0 && other.Id > 0)
            return Id == other.Id;
        return Code == other.Code;
    }

    public override int GetHashCode()
    {
        return Id > 0 ? Id.GetHashCode() : Code.GetHashCode();
    }

    public static bool operator ==(Role? left, Role? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Role? left, Role? right)
    {
        return !Equals(left, right);
    }
}

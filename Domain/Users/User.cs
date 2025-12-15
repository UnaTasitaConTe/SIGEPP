using Domain.Security.Entities;
using Domain.Security.ValueObjects;

namespace Domain.Users;

/// <summary>
/// Entity User - Representa un usuario del sistema SIGEPP.
/// Aggregate Root que gestiona la identidad, autenticación y autorización del usuario.
/// </summary>
public class User
{
    private readonly HashSet<Role> _roles = new();

    // Constructor privado para EF Core
    private User() { }

    /// <summary>
    /// Constructor para crear un nuevo usuario.
    /// </summary>
    private User(
        Guid id,
        string name,
        string email,
        string passwordHash,
        bool isActive,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del usuario no puede estar vacío.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario no puede estar vacío.", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("El formato del email no es válido.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de contraseña no puede estar vacío.", nameof(passwordHash));

        Id = id;
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Email del usuario (único en el sistema).
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña del usuario (nunca se guarda en texto plano).
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Indica si el usuario está activo en el sistema.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Fecha de creación del usuario.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha de última actualización del usuario.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Colección de roles asignados al usuario.
    /// </summary>
    public IReadOnlyCollection<Role> Roles => _roles.ToList().AsReadOnly();

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    public static User Create(
        string name,
        string email,
        string passwordHash,
        bool isActive = true)
    {
        return new User(
            Guid.NewGuid(),
            name,
            email,
            passwordHash,
            isActive,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Crea un usuario con un ID específico (útil para migraciones o seeds).
    /// </summary>
    public static User CreateWithId(
        Guid id,
        string name,
        string email,
        string passwordHash,
        bool isActive = true)
    {
        return new User(
            id,
            name,
            email,
            passwordHash,
            isActive,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Activa al usuario en el sistema.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desactiva al usuario en el sistema.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el nombre del usuario.
    /// </summary>
    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del usuario no puede estar vacío.", nameof(name));

        var trimmedName = name.Trim();
        if (Name == trimmedName)
            return;

        Name = trimmedName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el email del usuario.
    /// </summary>
    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario no puede estar vacío.", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("El formato del email no es válido.", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (Email == normalizedEmail)
            return;

        Email = normalizedEmail;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Establece el hash de la contraseña del usuario.
    /// El hash debe calcularse externamente usando IPasswordHasher.
    /// </summary>
    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de contraseña no puede estar vacío.", nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Asigna un rol al usuario si no lo tiene ya.
    /// </summary>
    public void AssignRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_roles.Any(r => r.Code == role.Code))
            return; // Ya tiene el rol

        _roles.Add(role);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Quita un rol del usuario si lo tiene.
    /// </summary>
    public void RemoveRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var existingRole = _roles.FirstOrDefault(r => r.Code == role.Code);
        if (existingRole == null)
            return; // No tiene el rol

        _roles.Remove(existingRole);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Quita todos los roles del usuario.
    /// </summary>
    public void ClearRoles()
    {
        if (_roles.Count == 0)
            return;

        _roles.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica si el usuario tiene un rol específico.
    /// </summary>
    public bool HasRole(string roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            return false;

        return _roles.Any(r => r.Code.Equals(roleCode, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifica si el usuario tiene un permiso específico a través de sus roles.
    /// </summary>
    public bool HasPermission(Permission permission)
    {
        if (permission == null)
            return false;

        return _roles.Any(role => role.HasPermission(permission));
    }

    /// <summary>
    /// Verifica si el usuario tiene un permiso específico (por código string).
    /// </summary>
    public bool HasPermission(string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;

        return _roles.Any(role => role.HasPermission(permissionCode));
    }

    /// <summary>
    /// Verifica si el usuario tiene al menos uno de los permisos especificados.
    /// </summary>
    public bool HasAnyPermission(IEnumerable<Permission> permissions)
    {
        if (permissions == null || !permissions.Any())
            return false;

        return permissions.Any(HasPermission);
    }

    /// <summary>
    /// Verifica si el usuario tiene todos los permisos especificados.
    /// </summary>
    public bool HasAllPermissions(params Permission[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            return false;

        return permissions.All(HasPermission);
    }

    /// <summary>
    /// Obtiene todos los permisos del usuario (unión de todos sus roles).
    /// </summary>
    public IReadOnlyCollection<Permission> GetAllPermissions()
    {
        return _roles
            .SelectMany(role => role.Permissions)
            .Distinct()
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Valida el formato básico de un email.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }
}

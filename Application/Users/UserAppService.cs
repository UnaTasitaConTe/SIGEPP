using Application.Security;
using Application.Users.Commands;
using Application.Users.DTOs;
using Domain.Common;
using Domain.Security.Repositories;
using Domain.Users;

namespace Application.Users;

/// <summary>
/// Servicio de aplicación para gestión de usuarios en SIGEPP.
/// Orquesta los casos de uso relacionados con usuarios.
/// </summary>
public class UserAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserAppService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="command">Comando con los datos del usuario a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del usuario creado.</returns>
    /// <exception cref="InvalidOperationException">Si el email ya está en uso.</exception>
    public async Task<Guid> CreateUserAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Validar que el email no exista
        var emailExists = await _userRepository.EmailExistsAsync(command.Email, ct);
        if (emailExists)
            throw new InvalidOperationException($"El email '{command.Email}' ya está en uso.");

        // Hashear la contraseña
        var passwordHash = _passwordHasher.HashPassword(command.RawPassword);

        // Crear el usuario usando el método de fábrica del dominio
        var user = User.Create(
            name: command.Name,
            email: command.Email,
            passwordHash: passwordHash,
            isActive: command.IsActive);

        // Asignar roles si se especificaron
        if (command.RoleCodes != null && command.RoleCodes.Any())
        {
            foreach (var roleCode in command.RoleCodes)
            {
                var role = await _roleRepository.GetByCodeAsync(roleCode, ct);
                if (role == null)
                    throw new InvalidOperationException($"El rol '{roleCode}' no existe.");

                user.AssignRole(role);
            }
        }

        // Guardar el usuario
        await _userRepository.AddAsync(user, ct);

        return user.Id;
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <param name="command">Comando con los datos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el usuario no existe o el email está en uso.</exception>
    public async Task UpdateUserAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Cargar el usuario
        var user = await _userRepository.GetByIdAsync(command.Id, ct);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID '{command.Id}' no existe.");

        // Validar que el email no esté en uso por otro usuario
        var emailExists = await _userRepository.EmailExistsAsync(command.Email, command.Id, ct);
        if (emailExists)
            throw new InvalidOperationException($"El email '{command.Email}' ya está en uso por otro usuario.");

        // Actualizar nombre y email usando métodos de dominio
        user.ChangeName(command.Name);
        user.ChangeEmail(command.Email);

        // Actualizar roles si se especificaron
        if (command.RoleCodes != null)
        {
            // Limpiar roles actuales
            user.ClearRoles();

            // Asignar nuevos roles
            foreach (var roleCode in command.RoleCodes)
            {
                var role = await _roleRepository.GetByCodeAsync(roleCode, ct);
                if (role == null)
                    throw new InvalidOperationException($"El rol '{roleCode}' no existe.");

                user.AssignRole(role);
            }
        }

        // Guardar cambios
        await _userRepository.UpdateAsync(user, ct);
    }

    /// <summary>
    /// Activa un usuario en el sistema.
    /// </summary>
    /// <param name="userId">ID del usuario a activar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el usuario no existe.</exception>
    public async Task ActivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID '{userId}' no existe.");

        user.Activate();
        await _userRepository.UpdateAsync(user, ct);
    }

    /// <summary>
    /// Desactiva un usuario en el sistema.
    /// </summary>
    /// <param name="userId">ID del usuario a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el usuario no existe.</exception>
    public async Task DeactivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID '{userId}' no existe.");

        user.Deactivate();
        await _userRepository.UpdateAsync(user, ct);
    }

    /// <summary>
    /// Asigna roles a un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="roleCodes">Códigos de roles a asignar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el usuario o algún rol no existe.</exception>
    public async Task AssignRolesAsync(Guid userId, IEnumerable<string> roleCodes, CancellationToken ct = default)
    {
        if (roleCodes == null || !roleCodes.Any())
            return;

        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID '{userId}' no existe.");

        foreach (var roleCode in roleCodes)
        {
            var role = await _roleRepository.GetByCodeAsync(roleCode, ct);
            if (role == null)
                throw new InvalidOperationException($"El rol '{roleCode}' no existe.");

            user.AssignRole(role);
        }

        await _userRepository.UpdateAsync(user, ct);
    }

    /// <summary>
    /// Quita un rol de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="roleCode">Código del rol a quitar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el usuario o el rol no existe.</exception>
    public async Task RemoveRoleAsync(Guid userId, string roleCode, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID '{userId}' no existe.");

        var role = await _roleRepository.GetByCodeAsync(roleCode, ct);
        if (role == null)
            throw new InvalidOperationException($"El rol '{roleCode}' no existe.");

        user.RemoveRole(role);
        await _userRepository.UpdateAsync(user, ct);
    }

    /// <summary>
    /// Obtiene todos los permisos efectivos de un usuario (unión de permisos de todos sus roles).
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de códigos de permisos del usuario.</returns>
    /// <exception cref="InvalidOperationException">Si el usuario no existe.</exception>
    public async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID '{userId}' no existe.");

        var permissions = user.GetAllPermissions()
            .Select(p => p.Value)
            .OrderBy(p => p)
            .ToList()
            .AsReadOnly();

        return permissions;
    }

    /// <summary>
    /// Cambia la contraseña de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="newRawPassword">Nueva contraseña en texto plano.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el usuario no existe.</exception>
    public async Task ChangePasswordAsync(Guid userId, string newRawPassword, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID '{userId}' no existe.");

        var newPasswordHash = _passwordHasher.HashPassword(newRawPassword);
        user.SetPasswordHash(newPasswordHash);

        await _userRepository.UpdateAsync(user, ct);
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="permissionCode">Código del permiso (ej: "ppa.create").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si el usuario tiene el permiso.</returns>
    /// <exception cref="InvalidOperationException">Si el usuario no existe.</exception>
    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
            throw new InvalidOperationException($"El usuario con ID '{userId}' no existe.");

        return user.HasPermission(permissionCode);
    }

    /// <summary>
    /// Obtiene un usuario por su ID con información detallada.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>DTO con información detallada del usuario o null si no existe.</returns>
    public async Task<UserDetailDto?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        return user == null ? null : MapToDetailDto(user);
    }

    /// <summary>
    /// Obtiene un usuario por su email con información detallada.
    /// </summary>
    /// <param name="email">Email del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>DTO con información detallada del usuario o null si no existe.</returns>
    public async Task<UserDetailDto?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, ct);
        return user == null ? null : MapToDetailDto(user);
    }

    /// <summary>
    /// Obtiene todos los usuarios activos.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de DTOs de usuarios activos.</returns>
    public async Task<IReadOnlyCollection<UserDto>> GetAllActiveUsersAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllActiveAsync(ct);
        return users.Select(MapToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todos los usuarios del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de DTOs de todos los usuarios.</returns>
    public async Task<IReadOnlyCollection<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(MapToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene una lista paginada de usuarios con filtros opcionales.
    /// </summary>
    /// <param name="query">Filtros de paginación y búsqueda para usuarios.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado paginado con usuarios.</returns>
    public async Task<PagedResult<UserDto>> GetUsersPagedAsync(UserPagedQuery query, CancellationToken ct = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var pagedResult = await _userRepository.GetPagedAsync(
            page: query.Page,
            pageSize: query.PageSize,
            search: query.Search,
            isActive: query.IsActive,
            roleCode: query.RoleCode,
            ct: ct);

        // Mapear las entidades de dominio a DTOs
        var dtos = pagedResult.Items.Select(MapToDto).ToList().AsReadOnly();

        return new PagedResult<UserDto>(
            items: dtos,
            page: pagedResult.Page,
            pageSize: pagedResult.PageSize,
            totalItems: pagedResult.TotalItems);
    }

    // ====================================
    // Métodos de mapeo privados
    // ====================================

    /// <summary>
    /// Mapea una entidad User a UserDto (información básica).
    /// </summary>
    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            IsActive = user.IsActive,
            Roles = user.Roles.Select(r => r.Code).ToList()
        };
    }

    /// <summary>
    /// Mapea una entidad User a UserDetailDto (información completa).
    /// </summary>
    private static UserDetailDto MapToDetailDto(User user)
    {
        return new UserDetailDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = user.Roles.Select(r => new UserRoleDto
            {
                Code = r.Code,
                Name = r.Name,
                Description = r.Description
            }).ToList(),
            Permissions = user.GetAllPermissions().Select(p => p.Value).ToList()
        };
    }
}

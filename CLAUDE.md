# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SIGEPP** (FESC-PPA Hub) is an ASP.NET Core Web API implementing a Role-Based Access Control (RBAC) security system for an academic management platform. The system manages teaching projects, resources, subjects, and academic periods.

**Technology Stack:**
- .NET 10.0 with ASP.NET Core Web API
- Entity Framework Core 10.0.1
- PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL)
- Docker support (Alpine Linux base)

## Architecture

This project follows **Clean Architecture** with **Domain-Driven Design (DDD)** principles. Dependencies flow inward: Presentation → Application → Domain ← Infrastructure.

### Layer Structure

```
SIGEPP/
├── Domain/          # Pure business logic, zero infrastructure dependencies
├── Application/     # Use cases, commands/queries (CQRS pattern)
├── Infrastructure/  # Data persistence (EF Core), external services
└── SIGEPP/         # Presentation layer (API controllers, startup)
```

**Critical Rule:** Domain layer must NEVER reference Infrastructure or Application layers.

### Domain Layer (Domain/Security/ and Domain/Users/)

Contains pure domain logic with DDD patterns:

**Security Module (Domain/Security/)**:
- **Entities/Role.cs**: Aggregate root with identity (Id, Code, Name, Description, IsSystemRole) and permissions collection
- **ValueObjects/Permission.cs**: Immutable value object in "module.action" format (e.g., "ppa.create")
- **Catalogs/**: Static catalogs defining 22 permissions across 6 modules and 3 predefined roles (ADMIN, DOCENTE, CONSULTA_INTERNA)
- **Repositories/**: Interface definitions only (IRoleRepository, IPermissionRepository)

**Users Module (Domain/Users/)**:
- **User.cs**: Aggregate root representing a user with identity, credentials, and role assignments
- **IUserRepository.cs**: Repository interface for user persistence operations

Key domain concepts:
- **Permission Value Object**: Immutable, no identity, format validation enforced at construction
- **Role Entity**: Has identity, contains permission collection, provides `HasPermission()` and `HasAllPermissions()` behavior
- **User Entity**: Aggregate root managing user identity, password hash, and role assignments. Provides authorization through `HasPermission()` by delegating to assigned roles
- **System Roles**: Marked with `IsSystemRole = true`, cannot be deleted

### Infrastructure Layer (Infrastructure/Persistence/)

Implements data persistence:

- **ApplicationDbContext.cs**: EF Core DbContext
- **Entities/**: EF Core entity classes (PermissionEntity, RoleEntity, RolePermissionEntity for many-to-many)
- **Configurations/**: Fluent API mappings (separate from domain entities)
- **Repositories/**: Concrete implementations of domain repository interfaces
- **Seeds/**: Database seed data (22 permissions, 3 roles, 27 role-permission assignments)
- **Migrations/**: EF Core migrations (currently at InitialSecurityModel)

**Important Pattern**: Infrastructure entities (e.g., PermissionEntity) are separate from domain entities. Repositories handle mapping between them.

### Application Layer (Application/)

Contains application services, commands, and cross-cutting concerns:

**Users Module (Application/Users/)**:
- **UserAppService.cs**: Orchestrates user management use cases (create, update, activate/deactivate, role assignment)
- **Commands/**: DTOs for user operations (CreateUserCommand, UpdateUserCommand)

**Security Module (Application/Security/)**:
- **IPasswordHasher.cs**: Interface for password hashing/verification (implementation in Infrastructure)

The Application layer orchestrates domain logic without containing business rules. It coordinates between repositories, domain entities, and external services.

### Presentation Layer (SIGEPP/)

- **Program.cs**: Startup configuration, automatic migration on startup via `dbContext.Database.Migrate()`
- **Controllers/**: API endpoints (currently empty, ready for implementation)
- **appsettings.Development.json**: PostgreSQL connection (localhost:5432, database: fescppa)

## Common Development Commands

### Building and Running

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run API (from solution root)
dotnet run --project SIGEPP\SIGEPP.csproj

# API runs on http://localhost:5129
# Docker container exposes port 8080
```

### Database Migrations

All migration commands must specify both Infrastructure project (where migrations live) and SIGEPP startup project:

```bash
# Create new migration
dotnet ef migrations add MigrationName --project Infrastructure\Infrastructure.csproj --startup-project SIGEPP\SIGEPP.csproj --context ApplicationDbContext --output-dir Persistence/Migrations

# Apply migrations to database
dotnet ef database update --project Infrastructure\Infrastructure.csproj --startup-project SIGEPP\SIGEPP.csproj --context ApplicationDbContext

# List applied migrations
dotnet ef migrations list --project Infrastructure --startup-project SIGEPP

# Generate SQL script from migrations
dotnet ef migrations script --project Infrastructure --startup-project SIGEPP --output migration.sql

# Remove last migration (if not applied)
dotnet ef migrations remove --project Infrastructure --startup-project SIGEPP

# Drop database
dotnet ef database drop --project Infrastructure --startup-project SIGEPP
```

**Note:** Migrations run automatically on application startup via `Program.cs`.

### Docker

```bash
# Build image
docker build -t sigepp .

# Run container
docker run -p 8080:8080 sigepp
```

## Database Schema

**PostgreSQL Database:** `fescppa` (localhost:5432)

### Tables

1. **Permissions** (22 records)
   - Id (BIGINT PK)
   - Code (VARCHAR 100, UNIQUE) - "module.action" format
   - Module (VARCHAR 50)
   - Action (VARCHAR 50)
   - Description (VARCHAR 500)

2. **Roles** (3 records)
   - Id (BIGINT PK)
   - Code (VARCHAR 50, UNIQUE)
   - Name (VARCHAR 100)
   - Description (VARCHAR 500)
   - IsSystemRole (BIT)

3. **RolePermissions** (27 records, many-to-many junction)
   - RoleId (BIGINT FK)
   - PermissionId (BIGINT FK)
   - Composite PK: (RoleId, PermissionId)

### Permission Modules

The system has 6 modules with 22 permissions total:

1. **Periods** (Academic periods): view, create, update, deactivate
2. **Subjects** (Courses): view, create, update, deactivate
3. **TeacherSubjects**: manage
4. **PPA** (Academic projects): view_all, view_own, create, update, change_status, upload_file
5. **Resources** (Attachments): view_all, view_own, create, update, delete
6. **Dashboard**: view, view_details

### Predefined Roles

- **ADMIN** (16 permissions): Full academic management access
- **DOCENTE** (10 permissions): Teacher access to own projects
- **CONSULTA_INTERNA** (3 permissions): Read-only internal queries

## Users and Authorization Model

### User Entity (Domain/Users/User.cs)

The `User` entity is an **aggregate root** in the domain that represents a user in SIGEPP. It manages user identity, authentication credentials, and role-based authorization.

**Key Properties:**
- `Id` (Guid): Unique identifier
- `Name` (string): Full name of the user
- `Email` (string): Email address (unique, normalized to lowercase)
- `PasswordHash` (string): Hashed password - **never stores passwords in plain text**
- `IsActive` (bool): Whether the user is active in the system
- `CreatedAt` / `UpdatedAt` (DateTime): Audit timestamps
- `Roles` (IReadOnlyCollection<Role>): Collection of assigned roles

**Security Design:**
- Passwords are NEVER stored in plain text
- Email addresses are normalized to lowercase for consistency
- Password hashing is delegated to `IPasswordHasher` (implemented in Infrastructure)
- Role assignments are managed through domain methods, ensuring invariants

### User Behavior (Domain Methods)

The User entity provides rich behavior following DDD principles:

**Identity Management:**
- `Activate()` / `Deactivate()`: Enable/disable user access
- `ChangeName(string name)`: Update user name with validation
- `ChangeEmail(string email)`: Update email with format validation
- `SetPasswordHash(string passwordHash)`: Update password hash (hash calculated externally)

**Role Management:**
- `AssignRole(Role role)`: Assign a role to the user (idempotent)
- `RemoveRole(Role role)`: Remove a role from the user
- `ClearRoles()`: Remove all roles
- `HasRole(string roleCode)`: Check if user has a specific role

**Authorization Queries:**
- `HasPermission(Permission permission)`: Returns true if any of the user's roles has the permission
- `HasPermission(string permissionCode)`: Check permission by code string
- `HasAnyPermission(IEnumerable<Permission> permissions)`: Check if user has at least one permission
- `HasAllPermissions(params Permission[] permissions)`: Check if user has all specified permissions
- `GetAllPermissions()`: Returns union of all permissions from all roles

**Factory Methods:**
- `User.Create(...)`: Creates a new user with auto-generated ID
- `User.CreateWithId(...)`: Creates user with specific ID (for seeds/migrations)

### User-Role-Permission Relationship

The authorization model in SIGEPP follows this hierarchy:

```
User → Roles → Permissions
```

**How it works:**
1. A **User** can have multiple **Roles** (e.g., a user might be both ADMIN and DOCENTE)
2. Each **Role** has multiple **Permissions** (e.g., ADMIN has 16 permissions)
3. A **User's effective permissions** = union of all permissions from all assigned roles
4. Authorization is checked via `user.HasPermission("ppa.create")` which delegates to the user's roles

**Example:**
```csharp
// User with DOCENTE role
var user = await userRepository.GetByIdAsync(userId);

// Check specific permission
bool canCreatePPA = user.HasPermission("ppa.create");  // true (DOCENTE has this)
bool canCreatePeriod = user.HasPermission("periods.create");  // false (DOCENTE doesn't have this)

// Check role
bool isTeacher = user.HasRole("DOCENTE");  // true

// Get all permissions
var allPermissions = user.GetAllPermissions();  // Returns 10 permissions from DOCENTE role
```

### UserAppService (Application Layer)

The `UserAppService` orchestrates user management use cases. It does NOT contain business logic - that lives in the domain.

**Key Operations:**
- `CreateUserAsync(CreateUserCommand)`: Creates a user, hashes password, assigns roles
- `UpdateUserAsync(UpdateUserCommand)`: Updates user info and optionally reassigns roles
- `ActivateUserAsync(userId)` / `DeactivateUserAsync(userId)`: Enable/disable users
- `AssignRolesAsync(userId, roleCodes)`: Assign multiple roles to a user
- `RemoveRoleAsync(userId, roleCode)`: Remove a role from a user
- `GetUserPermissionsAsync(userId)`: Get all effective permissions for a user
- `ChangePasswordAsync(userId, newPassword)`: Update user password
- `UserHasPermissionAsync(userId, permissionCode)`: Check if user has a permission

**Validation Logic:**
- Email uniqueness is validated before creation
- Email uniqueness is validated on update (excluding the user being updated)
- Roles must exist before assignment (validates against `IRoleRepository`)
- Password is hashed using `IPasswordHasher` before storing

### IPasswordHasher Interface

Located in `Application/Security/IPasswordHasher.cs`, this interface abstracts password hashing:

```csharp
public interface IPasswordHasher
{
    string HashPassword(string rawPassword);
    bool VerifyPassword(string rawPassword, string passwordHash);
}
```

**Implementation:** Will be in Infrastructure layer (e.g., using BCrypt, PBKDF2, or ASP.NET Core Identity's hasher)

### Usage Examples

**Creating a user with roles:**
```csharp
var command = new CreateUserCommand
{
    Name = "Juan Pérez",
    Email = "juan.perez@fesc.edu.co",
    RawPassword = "SecurePassword123!",
    RoleCodes = new[] { "DOCENTE" },
    IsActive = true
};

var userId = await userAppService.CreateUserAsync(command);
```

**Checking user permissions:**
```csharp
var user = await userRepository.GetByIdAsync(userId);

// Check specific permission
if (user.HasPermission(Permissions.PPA.Create))
{
    // User can create PPAs
}

// Check multiple permissions
if (user.HasAllPermissions(Permissions.PPA.ViewOwn, Permissions.PPA.Update))
{
    // User can view and update their own PPAs
}

// Get all effective permissions
var permissions = user.GetAllPermissions();
foreach (var permission in permissions)
{
    Console.WriteLine($"- {permission.Value}");  // e.g., "ppa.create"
}
```

**Managing user roles dynamically:**
```csharp
// Assign additional role
await userAppService.AssignRolesAsync(userId, new[] { "ADMIN" });

// Remove a role
await userAppService.RemoveRoleAsync(userId, "DOCENTE");

// Get user's current permissions
var currentPermissions = await userAppService.GetUserPermissionsAsync(userId);
// Returns: ["dashboard.view", "dashboard.view_details", "periods.create", ...]
```

**Activate/Deactivate users:**
```csharp
// Deactivate a user (e.g., when leaving the organization)
await userAppService.DeactivateUserAsync(userId);

// Reactivate later
await userAppService.ActivateUserAsync(userId);
```

## Key Design Patterns and Conventions

### Domain-Driven Design Principles

1. **No Infrastructure in Domain**: Domain layer has NO references to EF Core, databases, or external libraries
2. **Value Objects are Immutable**: Permission cannot be modified after creation
3. **Entities Have Identity**: Role is identified by Id, not its properties
4. **Rich Domain Models**: Behavior lives in domain entities (e.g., `role.HasPermission("ppa.create")`)
5. **Repository Pattern**: Abstract data access behind interfaces defined in Domain

### Dependency Injection

Infrastructure services registered via `builder.Services.AddInfrastructure()` in DependencyInjection.cs:
- ApplicationDbContext
- IRoleRepository → RoleRepository
- IPermissionRepository → PermissionRepository
- IUserRepository → UserRepository
- IPasswordHasher → PasswordHasher (BCrypt or ASP.NET Core Identity hasher)

Application services should be registered in Program.cs or a separate Application DI extension:
- UserAppService

### Permission Format Validation

All permissions MUST follow "module.action" format. Domain enforces this via `Permission.Create()` validation.

### Ownership Permissions

Permissions with `_own` suffix (e.g., `ppa.view_own`, `resources.view_own`) indicate user can only access their own resources. Ownership logic must be implemented in Application/API layers.

## Testing the System

No testing infrastructure is currently in place. When implementing tests:

1. **Unit Tests**: Test Domain entities/value objects (no infrastructure needed)
2. **Integration Tests**: Test Repository implementations with in-memory database
3. **E2E Tests**: Test API endpoints

Example test controller from MIGRATION_GUIDE.md:

```csharp
[ApiController]
[Route("api/[controller]")]
public class SecurityTestController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;

    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleRepository.GetAllAsync();
        return Ok(roles.Select(r => new {
            r.Code,
            r.Name,
            Permissions = r.Permissions.Select(p => p.Value)
        }));
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission(
        [FromQuery] string role,
        [FromQuery] string permission)
    {
        var roleEntity = await _roleRepository.GetByCodeAsync(role);
        bool hasPermission = roleEntity.HasPermission(permission);
        return Ok(new { Role = role, Permission = permission, HasPermission = hasPermission });
    }
}
```

## Development Workflow

### When Adding New Features

1. **Start in Domain**: Define entities, value objects, and repository interfaces
2. **Infrastructure**: Implement repositories, add EF configurations and migrations
3. **Application**: Create commands/queries (CQRS pattern)
4. **Presentation**: Add controllers/endpoints

### When Adding New Permissions

1. Add constant to `Domain/Security/Catalogs/Permissions.cs`
2. Create new migration with seed data for the permission
3. Update role definitions if needed

### When Modifying Database Schema

1. Change EF Core entity and/or configuration in Infrastructure
2. Run `dotnet ef migrations add` (see commands above)
3. Migration applies automatically on next app startup, or run `dotnet ef database update`

## Project Maturity

**Current Status:**
- ✅ Domain layer complete with DDD patterns (Security + Users)
- ✅ User entity with rich domain behavior (authorization, role management)
- ✅ UserAppService for user management use cases
- ✅ IPasswordHasher interface defined
- ✅ Infrastructure layer with EF Core and seeding (Security module)
- ✅ Database migrations ready (Security module)
- ✅ DI container configured
- ⚠️ User persistence in Infrastructure (pending: UserEntity, UserRepository, UserConfiguration, migrations)
- ⚠️ IPasswordHasher implementation in Infrastructure (pending: BCrypt or ASP.NET Core Identity)
- ❌ No controllers/endpoints implemented yet
- ❌ No authentication middleware (JWT)
- ❌ No authorization middleware (`[RequirePermission]` attributes)
- ❌ No testing framework

**Next Development Steps:**
1. **Infrastructure for Users**:
   - Create UserEntity for EF Core
   - Create UserRoleEntity for many-to-many relationship
   - Implement UserRepository
   - Implement PasswordHasher (using BCrypt or ASP.NET Core Identity)
   - Create migration for Users and UserRoles tables
   - Create seed data for initial admin user

2. **Authentication**:
   - Implement JWT token generation/validation
   - Create LoginCommand and AuthService
   - Add authentication middleware

3. **Authorization**:
   - Create `[RequirePermission]` attribute
   - Implement authorization middleware
   - Add authorization policies

4. **API Controllers**:
   - Users controller (CRUD, role assignment)
   - Auth controller (login, refresh token)
   - Roles controller (view roles and permissions)

5. **Testing**:
   - Unit tests for Domain entities (User, Role, Permission)
   - Integration tests for UserRepository
   - E2E tests for authentication/authorization flow

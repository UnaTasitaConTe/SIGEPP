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

### Domain Layer (Domain/Security/)

Contains pure domain logic with DDD patterns:

- **Entities/Role.cs**: Aggregate root with identity (Id, Code, Name, Description, IsSystemRole) and permissions collection
- **ValueObjects/Permission.cs**: Immutable value object in "module.action" format (e.g., "ppa.create")
- **Catalogs/**: Static catalogs defining 22 permissions across 6 modules and 3 predefined roles (ADMIN, DOCENTE, CONSULTA_INTERNA)
- **Repositories/**: Interface definitions only (IRoleRepository, IPermissionRepository)

Key domain concepts:
- **Permission Value Object**: Immutable, no identity, format validation enforced at construction
- **Role Entity**: Has identity, contains permission collection, provides `HasPermission()` and `HasAllPermissions()` behavior
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

Currently minimal - intended for CQRS commands/queries, application services, and DTOs. Ready for expansion.

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
- ✅ Domain layer complete with DDD patterns
- ✅ Infrastructure layer with EF Core and seeding
- ✅ Database migrations ready
- ✅ DI container configured
- ⚠️ Application layer mostly empty (ready for CQRS implementation)
- ❌ No controllers/endpoints implemented yet
- ❌ No authentication/authorization middleware
- ❌ No testing framework

**Next Development Steps:**
1. Implement Application layer (CQRS commands/queries)
2. Create API controllers for role and permission management
3. Add JWT authentication
4. Implement authorization middleware and `[RequirePermission]` attributes
5. Add User and UserRole entities for authentication
6. Implement unit/integration/E2E tests

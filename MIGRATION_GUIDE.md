# 🚀 Guía de Migración - Sistema de Seguridad

## ✅ Pre-requisitos

- ✓ .NET 10.0 instalado
- ✓ EF Core 10.0.1 instalado en Infrastructure
- ✓ SQL Server disponible (LocalDB, Express o Full)

## 📦 Paso 1: Restaurar Paquetes

```bash
cd C:\Users\thoma\source\repos\SIGEPP
dotnet restore
```

## 🛠️ Paso 2: Configurar Connection String

### **Opción A: appsettings.json (Recomendado para desarrollo)**

Crear/editar `SIGEPP/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FESCPPA;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### **Opción B: SQL Server Express**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=FESCPPA;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

### **Opción C: SQL Server con credenciales**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FESCPPA;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  }
}
```

## ⚙️ Paso 3: Registrar DbContext en Program.cs

Editar `SIGEPP/Program.cs`:

```csharp
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Domain.Security.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("Infrastructure")
    )
);

// Registrar Repositorios
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

// ... resto de servicios
```

## 🔧 Paso 4: Instalar EF Core Tools (si no está instalado)

```bash
dotnet tool install --global dotnet-ef --version 10.0.1
```

O actualizar:

```bash
dotnet tool update --global dotnet-ef --version 10.0.1
```

Verificar instalación:

```bash
dotnet ef --version
```

Debe mostrar: `Entity Framework Core .NET Command-line Tools 10.0.1`

## 🗃️ Paso 5: Crear Migración Inicial

```bash
cd C:\Users\thoma\source\repos\SIGEPP

dotnet ef migrations add InitialSecurityModel ^
  --project Infrastructure\Infrastructure.csproj ^
  --startup-project SIGEPP\SIGEPP.csproj ^
  --context ApplicationDbContext ^
  --output-dir Persistence/Migrations
```

### **Salida esperada:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

### **Archivos creados:**
```
Infrastructure/Persistence/Migrations/
  ├── 20250101000000_InitialSecurityModel.cs
  ├── 20250101000000_InitialSecurityModel.Designer.cs
  └── ApplicationDbContextModelSnapshot.cs
```

## ✅ Paso 6: Revisar la Migración

Abrir `Infrastructure/Persistence/Migrations/[timestamp]_InitialSecurityModel.cs`:

Debe contener:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Crear tabla Permissions
    migrationBuilder.CreateTable(
        name: "Permissions",
        columns: table => new
        {
            Id = table.Column<long>(type: "bigint", nullable: false),
            Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            // ...
        });

    // 2. Crear tabla Roles
    migrationBuilder.CreateTable(
        name: "Roles",
        // ...
    );

    // 3. Crear tabla RolePermissions
    migrationBuilder.CreateTable(
        name: "RolePermissions",
        // ...
    );

    // 4. Insertar 22 permisos (seed data)
    migrationBuilder.InsertData(
        table: "Permissions",
        // ...
    );

    // 5. Insertar 3 roles (seed data)
    migrationBuilder.InsertData(
        table: "Roles",
        // ...
    );

    // 6. Insertar 27 relaciones role-permission
    migrationBuilder.InsertData(
        table: "RolePermissions",
        // ...
    );
}
```

## 🚀 Paso 7: Aplicar Migración a Base de Datos

```bash
dotnet ef database update ^
  --project Infrastructure\Infrastructure.csproj ^
  --startup-project SIGEPP\SIGEPP.csproj ^
  --context ApplicationDbContext
```

### **Salida esperada:**
```
Build started...
Build succeeded.
Applying migration '20250101000000_InitialSecurityModel'.
Done.
```

## 🔍 Paso 8: Verificar Base de Datos

### **Opción A: SQL Server Management Studio (SSMS)**

Conectar a tu servidor y ejecutar:

```sql
USE FESCPPA;

-- Verificar tablas creadas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Resultado esperado:
-- Permissions
-- RolePermissions
-- Roles
-- __EFMigrationsHistory

-- Verificar permisos (debe haber 22)
SELECT COUNT(*) AS TotalPermisos FROM Permissions;
SELECT * FROM Permissions ORDER BY Module, Action;

-- Verificar roles (debe haber 3)
SELECT COUNT(*) AS TotalRoles FROM Roles;
SELECT * FROM Roles;

-- Verificar relaciones (debe haber 27)
SELECT
    r.Code AS Rol,
    r.Name AS NombreRol,
    COUNT(*) AS TotalPermisos
FROM Roles r
INNER JOIN RolePermissions rp ON r.Id = rp.RoleId
GROUP BY r.Code, r.Name
ORDER BY r.Code;

-- Resultado esperado:
-- ADMIN                 16 permisos
-- CONSULTA_INTERNA      3 permisos
-- DOCENTE               10 permisos
```

### **Opción B: Visual Studio (SQL Server Object Explorer)**

1. View → SQL Server Object Explorer
2. Expandir servidor → Databases → FESCPPA → Tables
3. Verificar tablas: `Permissions`, `Roles`, `RolePermissions`

### **Opción C: Azure Data Studio**

Similar a SSMS, ejecutar los queries SQL de arriba.

## 🧪 Paso 9: Probar Repositorios

Crear un controlador de prueba `SIGEPP/Controllers/SecurityTestController.cs`:

```csharp
using Domain.Security.Catalogs;
using Domain.Security.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SIGEPP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecurityTestController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public SecurityTestController(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    /// <summary>
    /// GET /api/securitytest/roles
    /// Listar todos los roles con sus permisos
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleRepository.GetAllAsync();

        var result = roles.Select(r => new
        {
            r.Id,
            r.Code,
            r.Name,
            r.Description,
            r.IsSystemRole,
            PermissionsCount = r.Permissions.Count,
            Permissions = r.Permissions.Select(p => p.Value).OrderBy(p => p).ToList()
        });

        return Ok(result);
    }

    /// <summary>
    /// GET /api/securitytest/roles/ADMIN
    /// Obtener un rol específico por código
    /// </summary>
    [HttpGet("roles/{code}")]
    public async Task<IActionResult> GetRoleByCode(string code)
    {
        var role = await _roleRepository.GetByCodeAsync(code);

        if (role == null)
            return NotFound($"Rol '{code}' no encontrado");

        return Ok(new
        {
            role.Id,
            role.Code,
            role.Name,
            role.Description,
            role.IsSystemRole,
            PermissionsCount = role.Permissions.Count,
            Permissions = role.Permissions.Select(p => p.Value).OrderBy(p => p).ToList()
        });
    }

    /// <summary>
    /// GET /api/securitytest/permissions
    /// Listar todos los permisos
    /// </summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _permissionRepository.GetAllAsync();

        var result = permissions.Select(p => new
        {
            p.Value,
            p.Module,
            p.Action
        });

        return Ok(result);
    }

    /// <summary>
    /// GET /api/securitytest/check?role=ADMIN&permission=ppa.create
    /// Verificar si un rol tiene un permiso
    /// </summary>
    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission(
        [FromQuery] string role,
        [FromQuery] string permission)
    {
        var roleEntity = await _roleRepository.GetByCodeAsync(role);

        if (roleEntity == null)
            return NotFound($"Rol '{role}' no encontrado");

        bool hasPermission = roleEntity.HasPermission(permission);

        return Ok(new
        {
            Role = role,
            Permission = permission,
            HasPermission = hasPermission,
            Message = hasPermission
                ? $"✅ {role} TIENE el permiso {permission}"
                : $"❌ {role} NO TIENE el permiso {permission}"
        });
    }
}
```

## 🔥 Paso 10: Probar la API

Ejecutar la aplicación:

```bash
cd C:\Users\thoma\source\repos\SIGEPP
dotnet run --project SIGEPP\SIGEPP.csproj
```

### **Pruebas con Swagger:**

Navegar a: `https://localhost:[port]/swagger`

### **Pruebas con curl/Postman:**

```bash
# 1. Listar todos los roles
curl https://localhost:5001/api/securitytest/roles

# 2. Obtener rol ADMIN
curl https://localhost:5001/api/securitytest/roles/ADMIN

# 3. Listar todos los permisos
curl https://localhost:5001/api/securitytest/permissions

# 4. Verificar si ADMIN puede crear períodos
curl "https://localhost:5001/api/securitytest/check?role=ADMIN&permission=periods.create"
# Resultado esperado: {"hasPermission": true}

# 5. Verificar si DOCENTE puede crear períodos
curl "https://localhost:5001/api/securitytest/check?role=DOCENTE&permission=periods.create"
# Resultado esperado: {"hasPermission": false}

# 6. Verificar si DOCENTE puede ver sus propios PPAs
curl "https://localhost:5001/api/securitytest/check?role=DOCENTE&permission=ppa.view_own"
# Resultado esperado: {"hasPermission": true}
```

## 🎯 Resultados Esperados

### **GET /api/securitytest/roles**
```json
[
  {
    "id": 1,
    "code": "ADMIN",
    "name": "Administrador",
    "description": "Acceso completo...",
    "isSystemRole": true,
    "permissionsCount": 16,
    "permissions": [
      "dashboard.view",
      "dashboard.view_details",
      "periods.create",
      // ... 13 más
    ]
  },
  // ... DOCENTE y CONSULTA_INTERNA
]
```

### **GET /api/securitytest/check?role=ADMIN&permission=ppa.create**
```json
{
  "role": "ADMIN",
  "permission": "ppa.create",
  "hasPermission": false,
  "message": "❌ ADMIN NO TIENE el permiso ppa.create"
}
```

**Nota:** ADMIN NO tiene `ppa.create`, solo tiene `ppa.view_all`, `ppa.update`, etc.

## 🔧 Comandos Útiles

### **Ver migraciones aplicadas:**
```bash
dotnet ef migrations list --project Infrastructure --startup-project SIGEPP
```

### **Generar script SQL:**
```bash
dotnet ef migrations script ^
  --project Infrastructure --startup-project SIGEPP ^
  --output migration.sql
```

### **Revertir migración:**
```bash
# Revertir última migración
dotnet ef database update [migración-anterior] --project Infrastructure --startup-project SIGEPP

# Eliminar migración (si no se ha aplicado)
dotnet ef migrations remove --project Infrastructure --startup-project SIGEPP
```

### **Eliminar base de datos:**
```bash
dotnet ef database drop --project Infrastructure --startup-project SIGEPP
```

## ❌ Problemas Comunes

### **Error: "Build failed"**
```bash
# Limpiar y recompilar
dotnet clean
dotnet build
```

### **Error: "Unable to resolve service for DbContext"**
- Verificar que `Program.cs` tenga `AddDbContext<ApplicationDbContext>()`
- Verificar que la connection string esté configurada

### **Error: "Login failed for user"**
- Verificar credenciales en connection string
- Verificar que SQL Server esté corriendo
- Para LocalDB: `sqllocaldb start mssqllocaldb`

### **Error: "Cannot create database"**
- Verificar permisos del usuario SQL
- Crear la BD manualmente: `CREATE DATABASE FESCPPA;`

## ✅ Checklist Final

- [ ] Paquetes NuGet instalados (EF Core 10.0.1)
- [ ] Connection string configurada
- [ ] DbContext registrado en Program.cs
- [ ] Repositorios registrados en DI
- [ ] Migración creada exitosamente
- [ ] Migración aplicada a BD
- [ ] 22 permisos insertados
- [ ] 3 roles insertados
- [ ] 27 relaciones role-permission insertadas
- [ ] API de prueba funciona correctamente

## 🚀 Siguientes Pasos

1. **Application Layer**
   - Crear Commands/Queries (CQRS)
   - Implementar servicios de autorización
   - Agregar validaciones

2. **Autenticación**
   - Implementar JWT
   - Crear tabla Users
   - Crear tabla UserRoles

3. **Autorización**
   - Crear middleware de permisos
   - Crear attributes [RequirePermission]
   - Implementar policies

4. **Testing**
   - Unit tests de Domain
   - Integration tests de Repositories
   - E2E tests de API

---

**📚 Documentación adicional:**
- `Domain/Security/README.md` - Guía de uso
- `Domain/Security/ARCHITECTURE.md` - Arquitectura completa

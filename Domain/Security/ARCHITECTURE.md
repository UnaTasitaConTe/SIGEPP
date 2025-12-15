# Arquitectura de Seguridad - FESC-PPA Hub

## 📋 Resumen

Sistema completo de roles y permisos implementado con **DDD**, **Arquitectura Hexagonal** y **EF Core**, respaldado en base de datos.

## 🏗️ Arquitectura Implementada

### ✅ **Separación de Capas (Hexagonal)**

```
┌─────────────────────────────────────────────────────────┐
│                        DOMAIN                            │
│  - Entities: Role                                        │
│  - Value Objects: Permission                             │
│  - Catalogs: Permissions, Roles (constantes)            │
│  - Repositories: IRoleRepository, IPermissionRepository  │
│                                                          │
│  ❌ SIN dependencias de infraestructura                  │
│  ❌ SIN anotaciones de EF Core                           │
└─────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE                         │
│  - Entities (EF): RoleEntity, PermissionEntity,          │
│                   RolePermissionEntity                   │
│  - Configurations: Fluent API para mapeo                 │
│  - DbContext: ApplicationDbContext                       │
│  - Seeds: PermissionSeed, RoleSeed, RolePermissionSeed  │
│  - Repositories: RoleRepository, PermissionRepository    │
│                                                          │
│  ✅ Implementa interfaces de Domain                      │
│  ✅ Maneja persistencia en BD                            │
└─────────────────────────────────────────────────────────┘
```

## 📦 Estructura de Archivos

### **Domain Layer**

```
Domain/Security/
├── Entities/
│   └── Role.cs                    # Entity con identidad (Id, Code, Name, Permissions)
├── ValueObjects/
│   └── Permission.cs              # Value Object inmutable (modulo.accion)
├── Catalogs/
│   ├── Permissions.cs             # Constantes de permisos (22 permisos)
│   └── Roles.cs                   # Constantes de roles (3 roles)
├── Repositories/
│   ├── IRoleRepository.cs         # Contrato para persistencia de roles
│   └── IPermissionRepository.cs   # Contrato para consulta de permisos
├── README.md                      # Documentación de uso
└── ARCHITECTURE.md                # Este archivo
```

### **Infrastructure Layer**

```
Infrastructure/Persistence/
├── Entities/
│   ├── PermissionEntity.cs        # Entity EF Core para Permissions
│   ├── RoleEntity.cs              # Entity EF Core para Roles
│   └── RolePermissionEntity.cs    # Entity EF Core para many-to-many
├── Configurations/
│   ├── PermissionConfiguration.cs # Fluent API para Permissions
│   ├── RoleConfiguration.cs       # Fluent API para Roles
│   └── RolePermissionConfiguration.cs # Fluent API para many-to-many
├── Seeds/
│   ├── SecuritySeed.cs            # Orquestador de seeds
│   ├── PermissionSeed.cs          # Seed de 22 permisos
│   ├── RoleSeed.cs                # Seed de 3 roles
│   └── RolePermissionSeed.cs      # Seed de relaciones (27 asignaciones)
├── Repositories/
│   ├── RoleRepository.cs          # Implementación de IRoleRepository
│   └── PermissionRepository.cs    # Implementación de IPermissionRepository
└── ApplicationDbContext.cs        # DbContext principal
```

## 🗄️ Modelo de Base de Datos

### **Tabla: Permissions**

| Columna       | Tipo         | Descripción                          |
|---------------|--------------|--------------------------------------|
| Id            | BIGINT (PK)  | Identificador único                  |
| Code          | VARCHAR(100) | Código único (ej: "ppa.create")      |
| Module        | VARCHAR(50)  | Módulo (ej: "ppa")                   |
| Action        | VARCHAR(50)  | Acción (ej: "create")                |
| Description   | VARCHAR(500) | Descripción opcional                 |

**Índices:**
- `IX_Permissions_Code` (UNIQUE)
- `IX_Permissions_Module_Action`

### **Tabla: Roles**

| Columna       | Tipo         | Descripción                          |
|---------------|--------------|--------------------------------------|
| Id            | BIGINT (PK)  | Identificador único                  |
| Code          | VARCHAR(50)  | Código único (ej: "ADMIN")           |
| Name          | VARCHAR(100) | Nombre legible                       |
| Description   | VARCHAR(500) | Descripción                          |
| IsSystemRole  | BIT          | Rol del sistema (no eliminable)      |

**Índices:**
- `IX_Roles_Code` (UNIQUE)
- `IX_Roles_IsSystemRole`

### **Tabla: RolePermissions** (Many-to-Many)

| Columna       | Tipo         | Descripción                          |
|---------------|--------------|--------------------------------------|
| RoleId        | BIGINT (FK)  | ID del rol                           |
| PermissionId  | BIGINT (FK)  | ID del permiso                       |

**PK Compuesta:** `(RoleId, PermissionId)`

**Índices:**
- `IX_RolePermissions_RoleId`
- `IX_RolePermissions_PermissionId`

## 📊 Datos Seed (Inicial)

### **22 Permisos Totales**

#### Gestión Académica (9 permisos)
```
periods.view, periods.create, periods.update, periods.deactivate
subjects.view, subjects.create, subjects.update, subjects.deactivate
teacherSubjects.manage
```

#### PPA/Proyectos (6 permisos)
```
ppa.view_all, ppa.view_own, ppa.create, ppa.update,
ppa.change_status, ppa.upload_file
```

#### Recursos/Anexos (5 permisos)
```
resources.view_all, resources.view_own, resources.create,
resources.update, resources.delete
```

#### Dashboard (2 permisos)
```
dashboard.view, dashboard.view_details
```

### **3 Roles con Asignaciones**

| Rol                | Permisos | Descripción                                    |
|--------------------|----------|------------------------------------------------|
| ADMIN              | 16       | Gestión académica completa, supervisión de PPAs|
| DOCENTE            | 10       | Gestión de sus propios PPAs y recursos         |
| CONSULTA_INTERNA   | 3        | Solo lectura de PPAs, recursos y dashboard     |

### **Matriz de Permisos por Rol**

Ver `Domain/Security/README.md` para la matriz completa.

## 🔄 Flujo de Uso

### **1. Inicialización (Primera vez)**

```bash
# Crear migración
dotnet ef migrations add InitialSecurityModel --project Infrastructure --startup-project SIGEPP

# Aplicar migración (crea tablas y aplica seeds)
dotnet ef database update --project Infrastructure --startup-project SIGEPP
```

Resultado:
- ✅ Tablas creadas: `Permissions`, `Roles`, `RolePermissions`
- ✅ 22 permisos insertados
- ✅ 3 roles insertados
- ✅ 27 relaciones role-permission insertadas

### **2. Consultar Roles y Permisos desde BD**

```csharp
using Domain.Security.Repositories;
using Domain.Security.Catalogs;

public class AuthorizationService
{
    private readonly IRoleRepository _roleRepository;

    public AuthorizationService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<bool> UserHasPermissionAsync(string userRoleCode, string permissionCode)
    {
        // 1. Obtener rol desde BD (incluye sus permisos)
        var role = await _roleRepository.GetByCodeAsync(userRoleCode);

        if (role == null)
            return false;

        // 2. Verificar permiso usando método de dominio
        return role.HasPermission(permissionCode);
    }

    // Ejemplo de uso con constantes del catálogo
    public async Task<bool> AdminCanCreatePeriodsAsync()
    {
        var admin = await _roleRepository.GetByCodeAsync(Roles.AdminId);
        return admin?.HasPermission(Permissions.Periods.Create) ?? false;
    }
}
```

### **3. Uso de Catálogos Estáticos**

Los catálogos (`Permissions`, `Roles`) se usan como **fuente de constantes**:

```csharp
// ✅ CORRECTO: Usar catálogos como constantes
public async Task CheckPermission()
{
    var role = await _roleRepository.GetByCodeAsync(Roles.AdminId); // "ADMIN"
    bool canCreate = role.HasPermission(Permissions.PPA.Create);    // "ppa.create"
}

// ❌ INCORRECTO: No usar roles en memoria directamente
var admin = Roles.Admin; // ⚠️ Este rol NO tiene permisos de BD
bool canCreate = admin.HasPermission(Permissions.PPA.Create); // ❌ Siempre false!
```

**Regla de oro:**
- Los catálogos son **solo para códigos constantes**
- La **fuente de verdad** es la **base de datos**
- Siempre recuperar roles desde `IRoleRepository`

## 🔐 Principios DDD Aplicados

### ✅ **1. Separación de Capas**
- Domain: Lógica de negocio pura (sin EF)
- Infrastructure: Detalles de persistencia (con EF)

### ✅ **2. Dependency Inversion**
- Domain define contratos (`IRoleRepository`)
- Infrastructure implementa contratos (`RoleRepository`)

### ✅ **3. Value Objects**
- `Permission` es inmutable, sin identidad
- Validación en tiempo de construcción

### ✅ **4. Entities**
- `Role` tiene identidad (`Id`, `Code`)
- Encapsula comportamiento (`HasPermission()`)

### ✅ **5. Repository Pattern**
- Abstrae persistencia
- Permite testing sin BD

### ✅ **6. Seed Data como Configuración**
- Seeds separados (SOLID - Single Responsibility)
- Basados en catálogos de dominio

## 🧪 Testing

### **Unit Tests (Domain)**

```csharp
[Fact]
public void Role_HasPermission_ReturnsTrueWhenPermissionExists()
{
    // Arrange
    var permission = Permission.Create("ppa.create");
    var role = Role.Create("ADMIN", "Admin", "Test", true, permission);

    // Act
    bool hasPermission = role.HasPermission(permission);

    // Assert
    Assert.True(hasPermission);
}
```

### **Integration Tests (Infrastructure)**

```csharp
[Fact]
public async Task RoleRepository_GetByCode_ReturnsRoleWithPermissions()
{
    // Arrange
    var context = CreateInMemoryDbContext();
    var repository = new RoleRepository(context);

    // Act
    var role = await repository.GetByCodeAsync("ADMIN");

    // Assert
    Assert.NotNull(role);
    Assert.Equal("ADMIN", role.Code);
    Assert.True(role.Permissions.Count > 0);
}
```

## 📝 Próximos Pasos

### **1. Application Layer**
- Crear casos de uso (Commands/Queries)
- Implementar servicios de autorización
- Agregar DTOs

### **2. Presentation Layer**
- Crear controladores/endpoints
- Implementar attributes de autorización
- Agregar middleware de permisos

### **3. Usuarios**
- Crear entity `User`
- Crear tabla `UserRoles` (many-to-many User-Role)
- Implementar autenticación JWT

### **4. Caché**
- Cachear permisos por usuario
- Invalidar caché al cambiar roles

### **5. Auditoría**
- Registrar intentos de acceso
- Logging de cambios en roles/permisos

## 🎯 Ventajas de esta Arquitectura

✅ **Domain limpio**: Sin dependencias de EF
✅ **Testeable**: Fácil de mockear repositorios
✅ **Extensible**: Agregar nuevos permisos es trivial
✅ **Type-safe**: Permisos como objetos, no strings
✅ **Single Source of Truth**: Base de datos en runtime
✅ **SOLID**: Seeders separados, repositorios especializados
✅ **Auditable**: Relaciones explícitas en BD
✅ **Mantenible**: Catálogos como documentación viva

## 📚 Referencias

- **DDD**: Domain-Driven Design by Eric Evans
- **Hexagonal Architecture**: Alistair Cockburn
- **Repository Pattern**: Martin Fowler
- **EF Core**: Microsoft Docs

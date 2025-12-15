# Security - Catálogo de Roles y Permisos

## 📋 Descripción

Este módulo contiene el catálogo de roles y permisos del sistema **FESC-PPA Hub**, diseñado siguiendo los principios de **Domain-Driven Design (DDD)** y **Arquitectura Hexagonal**.

## 🏗️ Estructura

```
Security/
├── ValueObjects/
│   └── Permission.cs          # Value Object para permisos (inmutable)
├── Entities/
│   └── Role.cs                # Entity para roles (con identidad)
└── Catalogs/
    ├── Permissions.cs         # Catálogo estático de permisos
    └── Roles.cs               # Catálogo estático de roles predefinidos
```

## 🎯 Conceptos Clave

### Permission (Value Object)
- **Inmutable**: Una vez creado, no puede modificarse
- **Sin identidad**: Se identifica por su valor, no por un ID
- **Formato**: `"modulo.accion"` (ej: `"ppa.create"`, `"subjects.view"`)
- **Validación**: Garantiza formato correcto en tiempo de construcción

### Role (Entity)
- **Con identidad**: Se identifica por su ID único
- **Agregación**: Contiene una colección de permisos
- **Comportamiento**: Provee métodos para verificar permisos

## 📦 Módulos y Permisos

### 1. Períodos Académicos (Periods)
```csharp
Permissions.Periods.View         // "periods.view"
Permissions.Periods.Create       // "periods.create"
Permissions.Periods.Update       // "periods.update"
Permissions.Periods.Deactivate   // "periods.deactivate"
```

### 2. Asignaturas/Materias (Subjects)
```csharp
Permissions.Subjects.View        // "subjects.view"
Permissions.Subjects.Create      // "subjects.create"
Permissions.Subjects.Update      // "subjects.update"
Permissions.Subjects.Deactivate  // "subjects.deactivate"
```

### 3. Asignación Docente-Materia (TeacherSubjects)
```csharp
Permissions.TeacherSubjects.Manage  // "teacherSubjects.manage"
```

### 4. PPA (Proyectos Académicos)
```csharp
Permissions.PPA.ViewAll       // "ppa.view_all"
Permissions.PPA.ViewOwn       // "ppa.view_own"
Permissions.PPA.Create        // "ppa.create"
Permissions.PPA.Update        // "ppa.update"
Permissions.PPA.ChangeStatus  // "ppa.change_status"
Permissions.PPA.UploadFile    // "ppa.upload_file"
```

### 5. Recursos/Anexos (Resources)
```csharp
Permissions.Resources.ViewAll  // "resources.view_all"
Permissions.Resources.ViewOwn  // "resources.view_own"
Permissions.Resources.Create   // "resources.create"
Permissions.Resources.Update   // "resources.update"
Permissions.Resources.Delete   // "resources.delete"
```

### 6. Dashboard (Panel de Seguimiento)
```csharp
Permissions.Dashboard.View         // "dashboard.view"
Permissions.Dashboard.ViewDetails  // "dashboard.view_details"
```

## 👥 Roles Predefinidos

### 1. ADMIN (Administrador)
- **ID**: `"ADMIN"`
- **Descripción**: Acceso completo a gestión académica y supervisión
- **Permisos**:
  - ✅ **Gestión Académica completa**:
    - `periods.*` (view, create, update, deactivate)
    - `subjects.*` (view, create, update, deactivate)
    - `teacherSubjects.manage`
  - ✅ **PPA - Supervisión**:
    - `ppa.view_all`
    - `ppa.update`
    - `ppa.change_status`
    - `ppa.upload_file`
  - ✅ **Recursos - Solo lectura**:
    - `resources.view_all`
  - ✅ **Dashboard - Completo**:
    - `dashboard.view`
    - `dashboard.view_details`

### 2. DOCENTE (Docente)
- **ID**: `"DOCENTE"`
- **Descripción**: Gestiona sus propios PPAs y recursos
- **Permisos**:
  - ✅ **Materias - Solo lectura**:
    - `subjects.view`
  - ✅ **PPA - Gestión propia**:
    - `ppa.view_own` (solo sus PPAs)
    - `ppa.create`
    - `ppa.update`
    - `ppa.change_status`
    - `ppa.upload_file`
  - ✅ **Recursos - CRUD propio**:
    - `resources.view_own` (solo sus recursos)
    - `resources.create`
    - `resources.update`
    - `resources.delete`

### 3. CONSULTA_INTERNA (Consulta Interna)
- **ID**: `"CONSULTA_INTERNA"`
- **Descripción**: Solo lectura para auditoría y consulta
- **Permisos**:
  - ✅ **PPA - Solo lectura completa**:
    - `ppa.view_all`
  - ✅ **Recursos - Solo lectura completa**:
    - `resources.view_all`
  - ✅ **Dashboard - Básico**:
    - `dashboard.view`

## 💻 Ejemplos de Uso

### Crear un permiso
```csharp
using Domain.Security.ValueObjects;

// Desde string con formato "modulo.accion"
var permiso = Permission.Create("ppa.create");

// Obtener propiedades
Console.WriteLine(permiso.Value);   // "ppa.create"
Console.WriteLine(permiso.Module);  // "ppa"
Console.WriteLine(permiso.Action);  // "create"
```

### Usar permisos del catálogo
```csharp
using Domain.Security.Catalogs;

// Acceder a permisos predefinidos
var permiso = Permissions.PPA.Create;
var todosLosPPA = Permissions.PPA.All;

// Obtener permisos por módulo
var permisosSubjects = Permissions.GetByModule("subjects");

// Todos los permisos del sistema
var todosLosPermisos = Permissions.All;
```

### Trabajar con roles
```csharp
using Domain.Security.Entities;
using Domain.Security.Catalogs;

// Obtener rol predefinido
var admin = Roles.Admin;
var docente = Roles.Docente;

// Verificar permisos individuales
bool puedeCrearPeriodos = admin.HasPermission(Permissions.Periods.Create);  // ✅ true
bool docentePuedeCrearPeriodos = docente.HasPermission(Permissions.Periods.Create); // ❌ false

// Verificar múltiples permisos
bool adminTieneGestionAcademica = admin.HasAllPermissions(
    Permissions.Periods.View,
    Permissions.Subjects.Create,
    Permissions.TeacherSubjects.Manage
); // ✅ true

// Obtener todos los roles
var todosLosRoles = Roles.All;
var nombresRoles = Roles.GetAllNames();  // ["Administrador", "Docente", "Consulta Interna"]
var idsRoles = Roles.GetAllIds();        // ["ADMIN", "DOCENTE", "CONSULTA_INTERNA"]

// Buscar rol por ID
var rol = Roles.GetById("ADMIN");
var existe = Roles.Exists("DOCENTE");  // true
```

### Escenarios de uso típicos

#### Validar acceso de un docente a un PPA
```csharp
var docente = Roles.Docente;

// El docente puede crear sus propios PPAs
bool puedeCrear = docente.HasPermission(Permissions.PPA.Create); // ✅ true

// El docente solo ve sus propios PPAs (no todos)
bool vePropio = docente.HasPermission(Permissions.PPA.ViewOwn); // ✅ true
bool veTodos = docente.HasPermission(Permissions.PPA.ViewAll);  // ❌ false
```

#### Validar acceso administrativo
```csharp
var admin = Roles.Admin;

// El admin puede gestionar toda la parte académica
bool gestionaAcademica = admin.HasAllPermissions(
    Permissions.Periods.Create,
    Permissions.Subjects.Update,
    Permissions.TeacherSubjects.Manage
); // ✅ true

// El admin puede supervisar PPAs pero no eliminarlos
bool supervisa = admin.HasPermission(Permissions.PPA.ViewAll); // ✅ true
```

#### Validar acceso de consulta interna
```csharp
var consulta = Roles.ConsultaInterna;

// Solo puede ver, no modificar
bool puedeVer = consulta.HasPermission(Permissions.PPA.ViewAll); // ✅ true
bool puedeEditar = consulta.HasPermission(Permissions.PPA.Update); // ❌ false
```

## 📊 Matriz de Permisos por Rol

| Permiso | ADMIN | DOCENTE | CONSULTA_INTERNA |
|---------|-------|---------|------------------|
| **Gestión Académica** | | | |
| periods.* | ✅ | ❌ | ❌ |
| subjects.* | ✅ | 👁️ view | ❌ |
| teacherSubjects.manage | ✅ | ❌ | ❌ |
| **PPAs** | | | |
| ppa.view_all | ✅ | ❌ | ✅ |
| ppa.view_own | ❌ | ✅ | ❌ |
| ppa.create | ❌ | ✅ | ❌ |
| ppa.update | ✅ | ✅ | ❌ |
| ppa.change_status | ✅ | ✅ | ❌ |
| ppa.upload_file | ✅ | ✅ | ❌ |
| **Recursos** | | | |
| resources.view_all | ✅ | ❌ | ✅ |
| resources.view_own | ❌ | ✅ | ❌ |
| resources.create | ❌ | ✅ | ❌ |
| resources.update | ❌ | ✅ | ❌ |
| resources.delete | ❌ | ✅ | ❌ |
| **Dashboard** | | | |
| dashboard.view | ✅ | ❌ | ✅ |
| dashboard.view_details | ✅ | ❌ | ❌ |

## ✅ Principios DDD Aplicados

### 1. Lenguaje Ubicuo
- Los nombres de permisos y roles reflejan el lenguaje del dominio académico
- `Periods`, `Subjects`, `PPA`, `Resources`, etc.

### 2. Value Objects
- `Permission` es inmutable y se identifica por su valor
- Validaciones en tiempo de construcción
- Igualdad basada en valor

### 3. Entities
- `Role` tiene identidad propia (ID)
- Encapsula comportamiento (HasPermission, HasAllPermissions, etc.)

### 4. Sin dependencias de infraestructura
- No hay referencias a bases de datos
- No hay anotaciones de ORM
- Catálogos estáticos en memoria

### 5. Invariantes del dominio
- Formato obligatorio `"modulo.accion"` para permisos
- Validación de campos requeridos
- Colecciones inmutables (IReadOnlySet, IReadOnlyList)

## 🔄 Próximos Pasos

En las capas superiores (Application, Infrastructure) se podrán:
- Implementar servicios de autorización
- Crear repositorios para persistencia de usuarios y sus roles
- Implementar decorators/filters para ASP.NET Core
- Agregar caché de permisos por usuario
- Implementar auditoría de accesos
- Validar ownership para permisos "view_own" y "resources.view_own"

## 📝 Notas de Diseño

1. **Sin Entity Framework**: Estas clases son POCOs puros, sin atributos de EF
2. **Sin dependencias**: La capa Domain no depende de ninguna otra
3. **Extensible**: Se pueden agregar nuevos módulos y permisos fácilmente
4. **Type-safe**: Los permisos son objetos, no strings mágicos
5. **Testeable**: Fácil de probar unitariamente sin infraestructura
6. **Permisos "Own" vs "All"**: Los permisos `view_own` y `view_all` permiten diferenciar entre acceso propio y acceso global, la lógica de ownership se implementará en capas superiores

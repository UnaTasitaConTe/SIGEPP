# CLAUDE.md – SIGEPP

## 1. Contexto rápido

Proyecto: **SIGEPP**  
Tipo: API .NET con **DDD + Arquitectura Hexagonal / Clean**  
Domino: Gestión de PPAs, asignaturas, docentes, evidencias y recursos académicos para la FESC.

Actores principales:
- **Docente**: Sube PPAs y recursos propios.
- **Administrador / Dirección**: Configura periodos, asignaturas, usuarios; consulta seguimiento.
- **Consulta interna**: Solo lectura (auditoría / coordinación).

Tecnologías clave:
- ASP.NET Core Web API
- Entity Framework Core (PostgreSQL)
- JWT para auth
- BCrypt para password hashing

---

## 2. Proyectos y dependencias (REGLA DE ORO)

Solución dividida en:

- `Sigepp.Domain`
- `Sigepp.Application`
- `Sigepp.Infrastructure`
- `Sigepp.Api` (o `SIGEPP`, proyecto Web API)

Dependencias permitidas:

- `Sigepp.Domain`  
  - ❌ NO depende de ningún proyecto.

- `Sigepp.Application`  
  - ✅ Depende solo de `Sigepp.Domain`.

- `Sigepp.Infrastructure`  
  - ✅ Depende de `Sigepp.Application`.  
  - ✅ Puede depender también de `Sigepp.Domain`.

- `Sigepp.Api`  
  - ✅ Depende de `Sigepp.Application`.  
  - ✅ Depende de `Sigepp.Infrastructure`.

> Si el usuario pide algo que rompa esto, DEBES corregirlo y proponer la alternativa correcta. No mezclar capas.

---

## 3. Responsabilidades por capa

### 3.1 `Sigepp.Domain`

Contiene SOLO lógica de negocio:

- Entidades: `User`, `Role`, `Ppa`, `Subject`, etc.
- Value Objects: `Permission` (`"module.action"`, ej. `ppa.create`).
- Reglas de dominio / invariantes.
- Interfaces de repositorio (puertos de dominio):
  - `IUserRepository`, `IRoleRepository`, `IPpaRepository`, etc.

NO puede tener:

- EF Core (`DbContext`, atributos de mapeo).
- ASP.NET, HTTP, controllers.
- `JwtOptions`, `IOptions`, `IConfiguration`.
- Bcrypt, JWT u otras libs externas.

### 3.2 `Sigepp.Application`

Orquesta casos de uso:

- Servicios / casos de uso:
  - `AuthService`, `UserAppService`, `PpaService`, etc.
- DTOs / commands / responses.
- Puertos de servicios transversales:
  - `IPasswordHasher`, `IJwtTokenGenerator`, (futuro `IEmailSender`, etc.).
- Modelo de usuario autenticado:
  - `CurrentUser` (UserId, Name, Email, Roles, Permissions).

Puede usar:

- Entidades / VOs de Domain.
- Interfaces de repositorio de Domain.

NO puede:

- Usar `DbContext`, EF Core ni SQL.
- Usar `IOptions<JwtOptions>`, `JwtOptions`, `IConfiguration`.
- Implementar repositorios ni servicios concretos.
- Definir controllers.

### 3.3 `Sigepp.Infrastructure`

Implementa los “detalles”:

- Persistencia:
  - `ApplicationDbContext`.
  - `UserRepository : IUserRepository`, `RoleRepository`, etc.
- Seguridad:
  - `BcryptPasswordHasher : IPasswordHasher`.
  - `JwtTokenGenerator : IJwtTokenGenerator`.
  - `JwtOptions` (config de JWT).
- Extensiones para DI:
  - `AddInfrastructure`, `AddSigeppSecurity`, `AddPersistence`, etc.

Puede usar:

- Application (interfaces).
- Domain (entidades).
- EF Core, JWT, BCrypt, libs externas.

NO debe:

- Definir casos de uso.
- Crear entidades de dominio nuevas que no estén en Domain.
- Tener controllers.

### 3.4 `Sigepp.Api`

Capa de presentación:

- `Program.cs`:
  - Registro de Application + Infrastructure.
  - Configuración JWT (`AddAuthentication().AddJwtBearer(...)`).
- Controllers / endpoints REST.
- Middleware (`UseAuthentication`, `UseAuthorization`, etc.).

Debe:

- Llamar a casos de uso de Application.
- No contener lógica de dominio compleja.

---

## 4. Seguridad y usuarios (resumen)

En **Domain**:
- `User`: Id, Name, Email, PasswordHash, IsActive, Roles.
- `Role`: Id, Code, Name, Description, permisos asociados.
- `Permission` (VO): `"module.action"`.
- Repos: `IUserRepository`, `IRoleRepository`, `IPermissionRepository`.

En **Application**:
- `CurrentUser`.
- `IPasswordHasher`, `IJwtTokenGenerator`.
- `AuthService` que:
  - Usa `IUserRepository`, `IPasswordHasher`, `IJwtTokenGenerator`.
  - Valida credenciales y emite token.

En **Infrastructure**:
- `PasswordHasher` (BCrypt) → `IPasswordHasher`.
- `JwtTokenGenerator` (System.IdentityModel.Tokens.Jwt) → `IJwtTokenGenerator`.
- `JwtOptions` + `IOptions<JwtOptions>`.

En **Api**:
- Configuración de JWT (Issuer, Audience, SigningKey, Expiration).
- Uso de `[Authorize]` según sea necesario.

---

## 5. Comportamiento esperado de Claude

1. **No romper la arquitectura**  
   - Si el usuario pide algo fuera de capa (ej. `IOptions<JwtOptions>` en Application o `DbContext` en Domain), debes decirle que no y proponer el diseño correcto.

2. **No hablar de instalación de NuGet**  
   - Asume que todos los paquetes necesarios ya están instalados.

3. **Siempre indicar proyecto y namespace**  
   - Cada vez que escribas código, especifica:  
     - Proyecto: Domain / Application / Infrastructure / Api  
     - Namespace correcto.

4. **Seguir el modelo de seguridad descrito**  
   - Permissions tipo `"module.action"`.  
   - Users → Roles → Permissions.  
   - JWT y hashing solo implementados en Infrastructure.

5. **Mantener estilo DDD + Clean Architecture**  
   - Entidades ricas, servicios de aplicación, repositorios, puertos/adaptadores.

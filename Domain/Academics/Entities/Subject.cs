namespace Domain.Academics.Entities;

/// <summary>
/// Entity Subject - Representa una asignatura o materia en SIGEPP.
/// Aggregate Root que gestiona la información de las asignaturas del currículo académico.
/// </summary>
public sealed class Subject
{
    // Constructor privado para EF Core
    private Subject() { }

    /// <summary>
    /// Constructor privado para crear una nueva asignatura.
    /// </summary>
    private Subject(
        Guid id,
        string code,
        string name,
        string? description,
        bool isActive,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código de la asignatura no puede estar vacío.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la asignatura no puede estar vacío.", nameof(name));

        Id = id;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Identificador único de la asignatura.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Código único de la asignatura (ej: "ISW-101", "MAT-201").
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre de la asignatura (ej: "Ingeniería de Software I").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Descripción opcional de la asignatura.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indica si la asignatura está activa en el sistema.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Fecha de creación de la asignatura.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha de última actualización de la asignatura.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Crea una nueva asignatura.
    /// </summary>
    /// <param name="code">Código único de la asignatura (ej: "ISW-101").</param>
    /// <param name="name">Nombre de la asignatura.</param>
    /// <param name="description">Descripción opcional de la asignatura.</param>
    /// <param name="isActive">Indica si la asignatura está activa (por defecto: true).</param>
    /// <returns>Nueva instancia de Subject.</returns>
    public static Subject Create(
        string code,
        string name,
        string? description = null,
        bool isActive = true)
    {
        return new Subject(
            Guid.NewGuid(),
            code,
            name,
            description,
            isActive,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Crea una asignatura con un ID específico (útil para migraciones o seeds).
    /// </summary>
    /// <param name="id">ID específico de la asignatura.</param>
    /// <param name="code">Código único de la asignatura.</param>
    /// <param name="name">Nombre de la asignatura.</param>
    /// <param name="description">Descripción opcional de la asignatura.</param>
    /// <param name="isActive">Indica si la asignatura está activa.</param>
    /// <returns>Nueva instancia de Subject con el ID especificado.</returns>
    public static Subject CreateWithId(
        Guid id,
        string code,
        string name,
        string? description = null,
        bool isActive = true)
    {
        return new Subject(
            id,
            code,
            name,
            description,
            isActive,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Activa la asignatura en el sistema.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desactiva la asignatura en el sistema.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el código de la asignatura.
    /// </summary>
    /// <param name="code">Nuevo código de la asignatura.</param>
    public void ChangeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código de la asignatura no puede estar vacío.", nameof(code));

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (Code == normalizedCode)
            return;

        Code = normalizedCode;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el nombre de la asignatura.
    /// </summary>
    /// <param name="name">Nuevo nombre de la asignatura.</param>
    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la asignatura no puede estar vacío.", nameof(name));

        var trimmedName = name.Trim();
        if (Name == trimmedName)
            return;

        Name = trimmedName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia la descripción de la asignatura.
    /// </summary>
    /// <param name="description">Nueva descripción de la asignatura.</param>
    public void ChangeDescription(string? description)
    {
        var trimmedDescription = description?.Trim();
        if (Description == trimmedDescription)
            return;

        Description = trimmedDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"{Name} ({Code})";
}

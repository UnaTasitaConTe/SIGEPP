namespace Domain.Academics.Entities;

/// <summary>
/// Entity AcademicPeriod - Representa un período académico en SIGEPP.
/// Aggregate Root que gestiona la información de períodos académicos (semestres, trimestres, etc.).
/// </summary>
public sealed class AcademicPeriod
{
    // Constructor privado para EF Core
    private AcademicPeriod() { }

    /// <summary>
    /// Constructor privado para crear un nuevo período académico.
    /// </summary>
    private AcademicPeriod(
        Guid id,
        string code,
        string name,
        DateOnly? startDate,
        DateOnly? endDate,
        bool isActive,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código del período académico no puede estar vacío.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del período académico no puede estar vacío.", nameof(name));

        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin.", nameof(startDate));

        Id = id;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Identificador único del período académico.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Código único del período académico (ej: "2024-1", "2024-2").
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del período académico (ej: "Periodo 2024-1").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio del período académico (opcional).
    /// </summary>
    public DateOnly? StartDate { get; private set; }

    /// <summary>
    /// Fecha de fin del período académico (opcional).
    /// </summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>
    /// Indica si el período académico está activo en el sistema.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Fecha de creación del período académico.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha de última actualización del período académico.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Crea un nuevo período académico.
    /// </summary>
    /// <param name="code">Código único del período (ej: "2024-1").</param>
    /// <param name="name">Nombre descriptivo del período.</param>
    /// <param name="startDate">Fecha de inicio (opcional).</param>
    /// <param name="endDate">Fecha de fin (opcional).</param>
    /// <param name="isActive">Indica si el período está activo (por defecto: true).</param>
    /// <returns>Nueva instancia de AcademicPeriod.</returns>
    public static AcademicPeriod Create(
        string code,
        string name,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool isActive = true)
    {
        return new AcademicPeriod(
            Guid.NewGuid(),
            code,
            name,
            startDate,
            endDate,
            isActive,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Crea un período académico con un ID específico (útil para migraciones o seeds).
    /// </summary>
    /// <param name="id">ID específico del período.</param>
    /// <param name="code">Código único del período.</param>
    /// <param name="name">Nombre descriptivo del período.</param>
    /// <param name="startDate">Fecha de inicio (opcional).</param>
    /// <param name="endDate">Fecha de fin (opcional).</param>
    /// <param name="isActive">Indica si el período está activo.</param>
    /// <returns>Nueva instancia de AcademicPeriod con el ID especificado.</returns>
    public static AcademicPeriod CreateWithId(
        Guid id,
        string code,
        string name,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool isActive = true)
    {
        return new AcademicPeriod(
            id,
            code,
            name,
            startDate,
            endDate,
            isActive,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Activa el período académico en el sistema.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desactiva el período académico en el sistema.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el código del período académico.
    /// </summary>
    /// <param name="code">Nuevo código del período.</param>
    public void ChangeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código del período académico no puede estar vacío.", nameof(code));

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (Code == normalizedCode)
            return;

        Code = normalizedCode;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el nombre del período académico.
    /// </summary>
    /// <param name="name">Nuevo nombre del período.</param>
    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del período académico no puede estar vacío.", nameof(name));

        var trimmedName = name.Trim();
        if (Name == trimmedName)
            return;

        Name = trimmedName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Establece las fechas del período académico.
    /// </summary>
    /// <param name="startDate">Fecha de inicio (opcional).</param>
    /// <param name="endDate">Fecha de fin (opcional).</param>
    public void SetDates(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin.", nameof(startDate));

        if (StartDate == startDate && EndDate == endDate)
            return;

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica si el período académico está vigente en una fecha específica.
    /// </summary>
    /// <param name="date">Fecha a verificar.</param>
    /// <returns>True si la fecha está dentro del rango del período, false en caso contrario.</returns>
    public bool IsCurrent(DateOnly date)
    {
        if (!IsActive)
            return false;

        // Si no tiene fechas definidas, no se puede determinar vigencia
        if (!StartDate.HasValue || !EndDate.HasValue)
            return false;

        return date >= StartDate.Value && date <= EndDate.Value;
    }

    /// <summary>
    /// Verifica si el período académico está vigente hoy.
    /// </summary>
    /// <returns>True si el período está vigente hoy.</returns>
    public bool IsCurrentToday()
    {
        return IsCurrent(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public override string ToString() => $"{Name} ({Code})";
}

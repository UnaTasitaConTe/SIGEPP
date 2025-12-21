namespace Domain.Ppa.ValueObjects;

/// <summary>
/// Representa un estudiante asociado a un PPA.
/// Este es un Value Object inmutable que contiene solo el nombre del estudiante.
/// </summary>
/// <remarks>
/// Al no referenciar entidades de usuarios, este VO permite asociar estudiantes
/// al PPA de forma simple, manteniendo el historial incluso si los estudiantes
/// no están dados de alta en el sistema.
/// </remarks>
public sealed class PpaStudent : IEquatable<PpaStudent>
{
    /// <summary>
    /// Identificador único del estudiante en el contexto del PPA.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Nombre completo del estudiante.
    /// </summary>
    public string Name { get; }

    private PpaStudent(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Crea un nuevo estudiante PPA.
    /// </summary>
    /// <param name="name">Nombre completo del estudiante.</param>
    /// <returns>Nueva instancia de PpaStudent.</returns>
    /// <exception cref="ArgumentException">Si el nombre está vacío.</exception>
    public static PpaStudent Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del estudiante es obligatorio.", nameof(name));

        return new PpaStudent(Guid.NewGuid(), name.Trim());
    }

    /// <summary>
    /// Factory para reconstruir un estudiante desde persistencia.
    /// </summary>
    /// <param name="id">ID del estudiante.</param>
    /// <param name="name">Nombre del estudiante.</param>
    /// <returns>Instancia de PpaStudent reconstruida.</returns>
    /// <exception cref="ArgumentException">Si el ID es vacío o el nombre está vacío.</exception>
    public static PpaStudent CreateWithId(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del estudiante no puede estar vacío.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del estudiante es obligatorio.", nameof(name));

        return new PpaStudent(id, name.Trim());
    }

    public bool Equals(PpaStudent? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is PpaStudent other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name.ToLowerInvariant());
    }

    public static bool operator ==(PpaStudent? left, PpaStudent? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PpaStudent? left, PpaStudent? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return Name;
    }
}

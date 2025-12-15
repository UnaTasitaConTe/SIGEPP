namespace Domain.Security.ValueObjects;

/// <summary>
/// Value Object que representa un permiso en el sistema.
/// Los permisos siguen el formato "modulo.accion" (ej: "ppas.crear", "anexos.editar").
/// Es inmutable y se identifica por su valor, no por identidad.
/// </summary>
public sealed class Permission : IEquatable<Permission>
{
    /// <summary>
    /// Valor del permiso en formato "modulo.accion"
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Módulo al que pertenece el permiso (ej: "ppas", "anexos")
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// Acción que representa el permiso (ej: "crear", "editar", "eliminar")
    /// </summary>
    public string Action { get; }

    private Permission(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El permiso no puede estar vacío", nameof(value));

        var parts = value.Split('.');
        if (parts.Length != 2)
            throw new ArgumentException(
                "El formato del permiso debe ser 'modulo.accion'",
                nameof(value));

        if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw new ArgumentException(
                "El módulo y la acción no pueden estar vacíos",
                nameof(value));

        Value = value.ToLowerInvariant();
        Module = parts[0].ToLowerInvariant();
        Action = parts[1].ToLowerInvariant();
    }

    /// <summary>
    /// Crea una nueva instancia de Permission a partir de un string
    /// </summary>
    public static Permission Create(string value)
    {
        return new Permission(value);
    }

    /// <summary>
    /// Crea una nueva instancia de Permission a partir de módulo y acción
    /// </summary>
    public static Permission Create(string module, string action)
    {
        return new Permission($"{module}.{action}");
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is Permission other && Equals(other);
    }

    public bool Equals(Permission? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(Permission? left, Permission? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Permission? left, Permission? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string(Permission permission) => permission.Value;
}

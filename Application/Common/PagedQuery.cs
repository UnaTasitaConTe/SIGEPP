namespace Application.Common;

/// <summary>
/// Modelo base para consultas paginadas con filtros comunes.
/// Puede ser extendido por consultas específicas que requieran filtros adicionales.
/// </summary>
public sealed record PagedQuery
{
    /// <summary>
    /// Número de página a consultar (base 1). Por defecto: 1.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Cantidad de elementos por página. Por defecto: 10.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Texto de búsqueda libre para filtrar por nombre, código, título, etc.
    /// según corresponda a cada entidad.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtro opcional por estado activo/inactivo.
    /// Si es null, no se aplica filtro por estado.
    /// Si es true, solo elementos activos.
    /// Si es false, solo elementos inactivos.
    /// </summary>
    public bool? IsActive { get; init; }
}

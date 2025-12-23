namespace Domain.Common;

/// <summary>
/// Representa un resultado paginado genérico que contiene una colección de elementos
/// junto con metadatos de paginación.
/// </summary>
/// <typeparam name="T">El tipo de elementos contenidos en la página.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Colección de elementos de la página actual.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; }

    /// <summary>
    /// Número de la página actual (base 1).
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Cantidad de elementos por página.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Cantidad total de elementos en todas las páginas.
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    /// Cantidad total de páginas disponibles.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Indica si existe una página anterior.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indica si existe una página siguiente.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Crea una nueva instancia de resultado paginado.
    /// </summary>
    /// <param name="items">Colección de elementos de la página actual.</param>
    /// <param name="page">Número de la página actual (base 1).</param>
    /// <param name="pageSize">Cantidad de elementos por página.</param>
    /// <param name="totalItems">Cantidad total de elementos en todas las páginas.</param>
    public PagedResult(
        IReadOnlyCollection<T> items,
        int page,
        int pageSize,
        int totalItems)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "El número de página debe ser mayor o igual a 1.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "El tamaño de página debe ser mayor o igual a 1.");

        if (totalItems < 0)
            throw new ArgumentOutOfRangeException(nameof(totalItems), "El total de elementos no puede ser negativo.");

        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    /// <summary>
    /// Crea un resultado paginado vacío.
    /// </summary>
    /// <param name="page">Número de página solicitado.</param>
    /// <param name="pageSize">Tamaño de página solicitado.</param>
    /// <returns>Un resultado paginado sin elementos.</returns>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 10)
    {
        return new PagedResult<T>(
            Array.Empty<T>(),
            page,
            pageSize,
            0);
    }
}

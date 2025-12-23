using Application.Common;

namespace Application.Users.DTOs;

/// <summary>
/// Consulta paginada especializada para usuarios con filtros adicionales.
/// </summary>
public sealed record UserPagedQuery
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
    /// Texto de búsqueda libre para filtrar por nombre o email del usuario.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtro opcional por estado activo/inactivo del usuario.
    /// Si es null, no se aplica filtro por estado.
    /// Si es true, solo usuarios activos.
    /// Si es false, solo usuarios inactivos.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filtro opcional por código de rol (ej: "ADMIN", "DOCENTE").
    /// Si se especifica, solo retorna usuarios que tengan ese rol asignado.
    /// </summary>
    public string? RoleCode { get; init; }
}

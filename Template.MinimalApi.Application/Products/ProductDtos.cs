namespace Template.MinimalApi.Application.Products;

public static class ProductDtos
{
    public sealed record Create(string Name, decimal Price);
    public sealed record Update(string Name, decimal Price);
    public sealed record Response(Guid Id, string Name, decimal Price, bool Active, DateTime CreatedAtUtc);

    public sealed record Query(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "name",
    bool Desc = false);

    public sealed record PagedResponse<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages);
}

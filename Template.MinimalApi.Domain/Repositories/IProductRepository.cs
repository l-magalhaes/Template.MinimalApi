using Template.MinimalApi.Domain.Entities;
using Template.MinimalApi.Domain.Abstractions;

namespace Template.MinimalApi.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<bool> NameExistsAsync(string name, Guid? ignoreId = null, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, string? sortBy, bool desc, CancellationToken ct = default);
}
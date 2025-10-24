using Microsoft.EntityFrameworkCore;
using Template.MinimalApi.Domain.Entities;
using Template.MinimalApi.Domain.Repositories;

namespace Template.MinimalApi.Infrastructure.Repositories;

public sealed class ProductRepository(AppDbContext db) : Repository<Product>(db), IProductRepository
{
    public async Task<bool> NameExistsAsync(string name, Guid? ignoreId = null, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking().Where(p => p.Name == name);
        if (ignoreId is Guid id) query = query.Where(p => p.Id != id);
        return await query.AnyAsync(ct);
    }

    public async Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, string? sortBy, bool desc, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var q = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => p.Name.Contains(s));
        }

        q = (sortBy?.ToLowerInvariant()) switch
        {
            "price" => (desc ? q.OrderByDescending(p => p.Price) : q.OrderBy(p => p.Price)),
            "created" => (desc ? q.OrderByDescending(p => p.CreatedAtUtc) : q.OrderBy(p => p.CreatedAtUtc)),
            _ => (desc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name)),
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
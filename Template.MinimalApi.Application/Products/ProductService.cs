using Template.MinimalApi.Domain.Entities;
using Template.MinimalApi.Domain.Repositories;
using Template.MinimalApi.Domain.Abstractions;
using Template.MinimalApi.Application.Products.Interfaces;

namespace Template.MinimalApi.Application.Products;

public sealed class ProductService(IProductRepository products, IUnitOfWork uow) : IProductService
{
    public async Task<ProductDtos.Response> CreateAsync(ProductDtos.Create dto, CancellationToken ct = default)
    {
        if (await products.NameExistsAsync(dto.Name, null, ct))
            throw new InvalidOperationException($"Product name '{dto.Name}' already exists.");

        var entity = new Product(dto.Name, dto.Price);
        await products.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return ProductMapper.ToResponse(entity);
    }

    public async Task<ProductDtos.Response?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await products.GetByIdAsync(id, ct);
        return entity is null ? null : ProductMapper.ToResponse(entity);
    }

    public async Task<ProductDtos.PagedResponse<ProductDtos.Response>> GetPagedAsync(ProductDtos.Query query, CancellationToken ct = default)
    {
        var (items, total) = await products.GetPagedAsync(query.Page, query.PageSize, query.Search, query.SortBy, query.Desc, ct);
        var dtoItems = items.Select(ProductMapper.ToResponse).ToList();
        var totalPages = (int)Math.Ceiling(total / (double)Math.Max(query.PageSize, 1));
        return new ProductDtos.PagedResponse<ProductDtos.Response>(dtoItems, query.Page, query.PageSize, total, totalPages);
    }

    public async Task<ProductDtos.Response?> UpdateAsync(Guid id, ProductDtos.Update dto, CancellationToken ct = default)
    {
        var entity = await products.GetByIdAsync(id, ct);
        if (entity is null) return null;

        if (await products.NameExistsAsync(dto.Name, id, ct))
            throw new InvalidOperationException($"Product name '{dto.Name}' already exists.");

        entity.Update(dto.Name, dto.Price);
        products.Update(entity);
        await uow.SaveChangesAsync(ct);
        return ProductMapper.ToResponse(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await products.GetByIdAsync(id, ct);
        if (entity is null) return false;
        products.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
using FluentValidation;
using Template.MinimalApi.Application.Products.Interfaces;
using Template.MinimalApi.Application.Products.Validator;
using Template.MinimalApi.Domain.Abstractions;
using Template.MinimalApi.Domain.Entities;
using Template.MinimalApi.Domain.Repositories;

namespace Template.MinimalApi.Application.Products;

public sealed class ProductService(IProductRepository productsRepository, IUnitOfWork unitOfWork) : IProductService
{
    public async Task<ProductDtos.Response> CreateAsync(ProductDtos.Create dto, CancellationToken ct = default)
    {
        var validator = new ProductCreateValidator();
        var result = validator.Validate(dto);

        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        if (await productsRepository.NameExistsAsync(dto.Name, null, ct))
            throw new InvalidOperationException($"Product name '{dto.Name}' already exists.");

        var product = new Product(dto.Name, dto.Price);
        await productsRepository.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ProductMapper.ToResponse(product);
    }

    public async Task<ProductDtos.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await productsRepository.GetByIdAsync(id, ct);
        return entity is null ? null : ProductMapper.ToResponse(entity);
    }

    public async Task<ProductDtos.PagedResponse<ProductDtos.Response>> GetAllPagedAsync(ProductDtos.Query query, CancellationToken ct = default)
    {
        var (items, total) = await productsRepository.GetPagedAsync(query.Page, query.PageSize, query.Search, query.SortBy, query.Desc, ct);
        var dtoItems = items.Select(ProductMapper.ToResponse).ToList();
        var totalPages = (int)Math.Ceiling(total / (double)Math.Max(query.PageSize, 1));
        return new ProductDtos.PagedResponse<ProductDtos.Response>(dtoItems, query.Page, query.PageSize, total, totalPages);
    }

    public async Task<ProductDtos.Response?> UpdateAsync(Guid id, ProductDtos.Update dto, CancellationToken ct = default)
    {
        var validator = new ProductUpdateValidator();
        var result = validator.Validate(dto);

        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var product = await productsRepository.GetByIdAsync(id, ct);
        if (product is null) return null;

        if (await productsRepository.NameExistsAsync(dto.Name, id, ct))
            throw new InvalidOperationException($"Product name '{dto.Name}' already exists.");

        product.Update(dto.Name, dto.Price);
        productsRepository.Update(product);
        await unitOfWork.SaveChangesAsync(ct);
        return ProductMapper.ToResponse(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await productsRepository.GetByIdAsync(id, ct);
        if (entity is null) return false;
        productsRepository.Remove(entity);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
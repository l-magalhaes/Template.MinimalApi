namespace Template.MinimalApi.Application.Products.Interfaces;

public interface IProductService
{
    Task<ProductDtos.Response> CreateAsync(ProductDtos.Create dto, CancellationToken ct = default);
    Task<ProductDtos.Response?> GetAsync(Guid id, CancellationToken ct = default);
    Task<ProductDtos.PagedResponse<ProductDtos.Response>> GetPagedAsync(ProductDtos.Query query, CancellationToken ct = default);
    Task<ProductDtos.Response?> UpdateAsync(Guid id, ProductDtos.Update dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

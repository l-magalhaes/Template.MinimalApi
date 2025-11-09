using Template.MinimalApi.Domain.Entities;

namespace Template.MinimalApi.Application.Products;

public static class ProductMapper
{
    public static ProductDtos.Response ToResponse(Product p)
        => new(p.Id, p.Name, p.Price, p.Active, p.CreatedAtUtc);
}
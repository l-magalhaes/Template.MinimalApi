using Microsoft.AspNetCore.Http.HttpResults;
using Template.MinimalApi.Application.Products;
using Template.MinimalApi.Application.Products.Interfaces;

namespace Template.MinimalApi.API;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("", async ([AsParameters] ProductDtos.Query query, IProductService svc, CancellationToken ct)
           => Results.Ok(await svc.GetPagedAsync(query, ct)))
           .WithName("GetProducts");

        group.MapGet("{id:guid}", async Task<Results<Ok<ProductDtos.Response>, NotFound>> (Guid id, IProductService svc, CancellationToken ct) =>
        {
            var res = await svc.GetAsync(id, ct);
            return res is null ? TypedResults.NotFound() : TypedResults.Ok(res);
        })
        .WithName("GetProductById");

        group.MapPost("", async Task<Results<Created<ProductDtos.Response>, BadRequest<string>>> (ProductDtos.Create dto, IProductService svc, CancellationToken ct) =>
        {
            try
            {
                var created = await svc.CreateAsync(dto, ct);
                return TypedResults.Created($"/api/products/{created.Id}", created);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        })
        .WithName("CreateProduct");

        group.MapPut("{id:guid}", async Task<Results<Ok<ProductDtos.Response>, NotFound, BadRequest<string>>> (Guid id, ProductDtos.Update dto, IProductService svc, CancellationToken ct) =>
        {
            try
            {
                var updated = await svc.UpdateAsync(id, dto, ct);
                return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        })
        .WithName("UpdateProduct");

        group.MapDelete("{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IProductService svc, CancellationToken ct) =>
        {
            var ok = await svc.DeleteAsync(id, ct);
            return ok ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteProduct");

        return app;
    }
}
using Microsoft.AspNetCore.Http.HttpResults;
using Template.MinimalApi.Application.Products;
using Template.MinimalApi.Application.Products.Interfaces;

namespace Template.MinimalApi.API;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("", async ([AsParameters] ProductDtos.Query query, IProductService productService, CancellationToken ct)
           => Results.Ok(await productService.GetAllPagedAsync(query, ct)))
           .WithName("GetProducts");

        group.MapGet("{id:guid}", async Task<Results<Ok<ProductDtos.Response>, NotFound>> (Guid id, IProductService productService, CancellationToken ct) =>
        {
            var res = await productService.GetByIdAsync(id, ct);
            return res is null ? TypedResults.NotFound() : TypedResults.Ok(res);
        })
        .WithName("GetProductById");

        group.MapPost("", async Task<Results<Created<ProductDtos.Response>, BadRequest<string>>> (ProductDtos.Create dto, IProductService productService, CancellationToken ct) =>
        {
            try
            {
                var created = await productService.CreateAsync(dto, ct);
                return TypedResults.Created($"/api/products/{created.Id}", created);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        })
        .WithName("CreateProduct");

        group.MapPut("{id:guid}", async Task<Results<Ok<ProductDtos.Response>, NotFound, BadRequest<string>>> (Guid id, ProductDtos.Update dto, IProductService productService, CancellationToken ct) =>
        {
            try
            {
                var updated = await productService.UpdateAsync(id, dto, ct);
                return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        })
        .WithName("UpdateProduct");

        group.MapDelete("{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IProductService productService, CancellationToken ct) =>
        {
            var ok = await productService.DeleteAsync(id, ct);
            return ok ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteProduct");

        return app;
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Template.MinimalApi.API;
using Template.MinimalApi.Application.Products;
using Template.MinimalApi.Application.Products.Interfaces;
using Template.MinimalApi.Domain.Abstractions;
using Template.MinimalApi.Domain.Repositories;
using Template.MinimalApi.Infrastructure;
using Template.MinimalApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Template.MinimalApi", Version = "v1" });
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(
        cfg.GetConnectionString("SqlServer"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null) 
    )
);

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Template.MinimalApi v1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }))
   .WithTags("Health");

app.MapProductEndpoints();

app.Run();

using Template.MinimalApi.Domain.Abstractions;

namespace Template.MinimalApi.Infrastructure;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Template.MinimalApi.Domain.Abstractions;

namespace Template.MinimalApi.Infrastructure.Repositories;

public class Repository<T>(AppDbContext db) : IRepository<T> where T : class
{
    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await db.Set<T>().AddAsync(entity, ct);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<T>().FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        IQueryable<T> q = db.Set<T>();
        if (predicate is not null) q = q.Where(predicate);
        return await q.AsNoTracking().ToListAsync(ct);
    }

    public void Remove(T entity) => db.Set<T>().Remove(entity);

    public void Update(T entity) => db.Set<T>().Update(entity);
}

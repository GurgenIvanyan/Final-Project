using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using User.Core.Interfaces.Repositories;
using User.Infrastructure.Persistence;

namespace User.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<TEntity> _set;

        public GenericRepository(AppDbContext db) { _db = db; _set = db.Set<TEntity>(); }

        public Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => _set.FindAsync(new object?[] { id }, ct).AsTask();

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().ToListAsync(ct);

        public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

        public Task AddAsync(TEntity entity, CancellationToken ct = default) { _set.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(TEntity entity, CancellationToken ct = default) { _set.Attach(entity); _db.Entry(entity).State = EntityState.Modified; return Task.CompletedTask; }
        public Task DeleteAsync(TEntity entity, CancellationToken ct = default) { _set.Remove(entity); return Task.CompletedTask; }
    }
}

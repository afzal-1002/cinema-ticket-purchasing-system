using System.Linq.Expressions;
using CinemaTicket.Config;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Repository;

public class Repository<TEntity>(ApplicationDbContext context) : IRepository<TEntity> where TEntity : class
{
    private readonly ApplicationDbContext _context = context;

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await _context.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

namespace PskOnline.Server.Shared.EFCore
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  using Microsoft.EntityFrameworkCore;

  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Shared.Repository;

  public class Repository<TEntity, TContext> : IGuidKeyedRepository<TEntity> where TEntity : BaseEntity where TContext : DbContext
  {
    protected readonly TContext _context;
    protected readonly DbSet<TEntity> _entities;

    public Repository(TContext context)
    {
      _context = context;
      _entities = context.Set<TEntity>();
    }

    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
    }

    public virtual void Add(TEntity entity)
    {
      _entities.Add(entity);
    }

    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
      _entities.AddRange(entities);
    }

    public virtual void Update(TEntity entity)
    {
      _entities.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
      _entities.UpdateRange(entities);
    }

    public virtual void Remove(TEntity entity)
    {
      _entities.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
      _entities.RemoveRange(entities);
    }

    public virtual int Count()
    {
      return _entities.Count();
    }

    public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
      return _entities.Where(predicate);
    }

    public virtual TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate)
    {
      return _entities.SingleOrDefault(predicate);
    }

    public virtual async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
      return await _entities.SingleOrDefaultAsync(predicate);
    }

    public virtual TEntity Get(Guid id)
    {
      return _entities.Find(id);
    }

    public virtual async Task<TEntity> GetAsync(Guid id)
    {
      return await _entities.FindAsync(id);
    }

    public virtual IQueryable<TEntity> Query(int? skip = null, int? take = null)
    {
      IQueryable<TEntity> query = _entities;
      if (skip.HasValue || take.HasValue)
      {
        query = query.OrderBy(item => item.CreatedDate);
        if (skip.HasValue)
        {
          query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
          query = query.Take(take.Value);
        }
      }

      return query;
    }
  }
}

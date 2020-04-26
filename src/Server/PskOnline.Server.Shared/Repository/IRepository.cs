namespace PskOnline.Server.Shared.Repository
{
  using PskOnline.Server.Shared.ObjectModel;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  public interface IGuidKeyedRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : class, IGuidIdentity
  {
  }

  public interface IRepository<TEntity, TKey> where TEntity : class
  {
    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);

    Task SaveChangesAsync();

    void Update(TEntity entity);

    void UpdateRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    int Count();

    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

    TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    TEntity Get(TKey id);

    Task<TEntity> GetAsync(TKey id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    /// <remarks>remember that the 'skip' or 'take' should be applied
    /// _after_ all other filter expressions ('Where()')</remarks>
    IQueryable<TEntity> Query(int? skip = null, int? take = null);
  }
}

namespace PskOnline.Server.Shared.Service
{
  using PskOnline.Server.Shared.ObjectModel;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public interface IService<TEntity>
    where TEntity : class, ITenantEntity, IGuidIdentity
  {
    Task<long> GetItemCountInTenantAsync(Guid tenantId);

    /// <summary>
    /// Adds the entity to the underlying repository.
    /// Assigns a unique ID if the entity doesn't have an ID.
    /// Exception will be thrown in case of an ID collision.
    /// Implementers may perform additional checks, such as 
    /// permission checks, duplicate checks etc.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<Guid> AddAsync(TEntity value);

    /// <summary>
    /// Retrieves the entity with the specified ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The entity</returns>
    /// <exception cref="UnauthorizedAccessException">In case the current user is not allowed to access the retrieved entity</exception>
    /// <exception cref="ItemNotFoundException">In case the specified id is not found</exception>
    Task<TEntity> GetAsync(Guid id);

    Task UpdateAsync(TEntity value);

    Task RemoveAsync(Guid id);

    Task<IEnumerable<TEntity>> GetAllAsync(int? skip, int? take);
  }
}

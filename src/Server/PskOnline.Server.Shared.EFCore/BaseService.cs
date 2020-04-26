namespace PskOnline.Server.Shared.EFCore
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.Service;

  /// <summary>
  /// The base implementation does the following:
  ///   * limits the scope of create/read/update/delete to a tenant
  ///     unless the 'current tenant' matches 'EntireSiteTenant' 
  ///     (which means that the current user is 'site user' and access check is performed
  ///     at application layer (taking into account roles, permissions etc.)
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  /// <typeparam name="TKey"></typeparam>
  public class BaseService<TEntity> : IService<TEntity>
    where TEntity : class, ITenantEntity, IGuidIdentity
  {
    private readonly IAccessChecker _tenantAccessChecker;
    private readonly IAccessScopeFilter _tenantContext;
    private readonly ILogger _logger;

    public BaseService(
      IAccessChecker tenantAccessChecker,
      IAccessScopeFilter tenantContext,
      IGuidKeyedRepository<TEntity> repository,
      ILogger<BaseService<TEntity>> logger)
    {
      _logger = logger;
      Repository = repository;
      _tenantAccessChecker = tenantAccessChecker;
      _tenantContext = tenantContext;
    }

    protected IGuidKeyedRepository<TEntity> Repository { get; set; }

    protected void CheckAccess(TEntity entity, EntityAction desiredAction)
    {
      _tenantAccessChecker.ValidateAccessToEntityAsync(entity, desiredAction);
    }

    protected virtual Task CheckEntityReferences(TEntity value)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// Should throw an ItemAlreadyExistsException if the new item is not unique
    /// E.g. a Department name is not unique within BranchOffice
    /// or Tenant name is not unique within the site
    /// or BranchOffice name is not unique within the Tenant
    /// </summary>
    /// <param name="value"></param>
    protected virtual Task CheckNewItemIsUnique(TEntity value)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// Should throw an ItemAlreadyExistsException if the new item is not unique
    /// E.g. a Department name is not unique within BranchOffice
    /// or Tenant name is not unique within the site
    /// or BranchOffice name is not unique within the Tenant
    /// </summary>
    /// <param name="value"></param>
    protected virtual Task CheckUpdatedItemIsUnique(TEntity value)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// Override this method if your specific entity stores some denormalized attributes
    /// that you need to update based on the key references. For example,
    /// Employee entity stores both DepartmentId and BranchOfficeId, with DepartmentId being the key
    /// and the BranchOfficeId being the denormalized parameter.
    /// </summary>
    /// <param name="employee"></param>
    /// <returns></returns>
    /// <remarks>May throw 'BadRequestException'</remarks>
    protected virtual Task UpdateDenormalizedParameters(TEntity value)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// 1. checks that the new item is unique 
    /// (override CheckNewItemIsUnique to add business logic)
    /// and that the new item refers to the correct entities
    /// (override CheckEntityReferenes)
    /// 2. adds the item to the repository
    /// 3. saves the changes
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async virtual Task<Guid> AddAsync(TEntity value)
    {
      CheckAccess(value, EntityAction.Create);
      await CheckNewItemIsUnique(value);
      await UpdateDenormalizedParameters(value);
      await CheckEntityReferences(value);
      if( value.Id == Guid.Empty )
      {
        value.Id = Guid.NewGuid();
      }
      Repository.Add(value);
      await Repository.SaveChangesAsync();
      return value.Id;
    }

    public TEntity Get(Guid id)
    {
      var entity = Repository.Get(id);
      if (entity != null)
      {
        CheckAccess(entity, EntityAction.Read);
        return entity;
      }
      throw new ItemNotFoundException(id.ToString(), typeof(TEntity).Name);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(int? skip, int? take)
    {
      var query = Repository.Query();
      query = AddScopeFilter(query);
      if (skip.HasValue)
      {
        query = query.Skip(skip.Value);
      }
      if (take.HasValue)
      {
        query = query.Take(take.Value);
      }
      return Task.FromResult((IEnumerable<TEntity>)query);
    }

    /// <summary>
    /// Adds a scope filtering expressions to the query
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    protected IQueryable<TEntity> AddScopeFilter(IQueryable<TEntity> query)
    {
      return _tenantContext.AddScopeFilter(query);
    }

    public async Task<TEntity> GetAsync(Guid id)
    {
      var entity = await Repository.GetAsync(id);
      if (entity != null)
      {
        CheckAccess(entity, EntityAction.Read);
        return entity;
      }
      throw new ItemNotFoundException(id.ToString(), typeof(TEntity).Name);
    }

    public virtual async Task UpdateAsync(TEntity value)
    {
      CheckAccess(value, EntityAction.Update);
      await UpdateDenormalizedParameters(value);
      await CheckEntityReferences(value);
      await CheckUpdatedItemIsUnique(value);
      Repository.Update(value);
      await Repository.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid id)
    {
      var entity = Repository.Get(id);
      if (entity != null)
      {
        CheckAccess(entity, EntityAction.Delete);
        Repository.Remove(entity);
        await Repository.SaveChangesAsync();
      }
    }

    public Task<long> GetItemCountInTenantAsync(Guid tenantId)
    {
      return Repository.Query().LongCountAsync(e => e.TenantId == tenantId);
    }

  }
}

namespace PskOnline.Server.Shared.Multitenancy
{
  using System.Linq;

  using PskOnline.Server.Shared.ObjectModel;

  public interface IAccessScopeFilter
  {
    /// <summary>
    /// adds a scope filter query to the query passed in as an argument,
    /// as required by the permissions of the current user
    /// /// </summary>
    /// <param name="entityQuery"></param>
    /// <returns></returns>
    IQueryable<T> AddScopeFilter<T>(IQueryable<T> entityQuery) where T : class, ITenantEntity;
  }
}

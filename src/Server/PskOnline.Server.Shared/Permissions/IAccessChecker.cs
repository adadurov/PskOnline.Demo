namespace PskOnline.Server.Shared.Permissions
{
  using System.Threading.Tasks;

  using PskOnline.Server.Shared.ObjectModel;

  /// <summary>
  /// Checks the following simple rules for accessing entities within tenants:
  /// 
  /// Create:
  ///  * Tenant User can create entity only within their own tenant
  ///  * Site User can create entity within any tenant
  /// 
  /// Access (read, modify)
  ///  * Site User can access entitites in any tenant, as well as entities outside any tenant
  ///  * Tenant User can access entities only within their own tenant
  ///  
  /// Delete:
  ///  * Tenant User can delete entity only within their own tenant
  ///  * Site User can delete entity within any tenant
  ///  
  /// BUG: Fine-grainded access permissions based on scope and role, are not yet implemented.
  /// </summary>
  public interface IAccessChecker
  {
    /// <summary>
    /// throws UnauthorizedAccessException if the current user
    /// (associated with the access checker instance) is not allowed
    /// to access entities within the entityTenantId
    /// </summary>
    /// <param name="entityId"></param>
    Task ValidateAccessToEntityAsync(ITenantEntity entity, EntityAction desiredAction);
  }
}

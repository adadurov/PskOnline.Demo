namespace PskOnline.Server.Authority.API.Constants
{
  public static class CustomClaimTypes
  {
    /// <summary>
    /// A claim that specifies a permission of an authenticated user
    /// </summary>
    public const string Permission = "permission";

    public const string TenantId = "tenantId";

    public const string BranchOfficeId = "branchOfficeId";

    public const string DepartmentId = "departmentId";

    /// <summary>
    /// A claim that defines the scope that the principal is defined at
    /// (either Individual, Department, BranchOffice, Tenant or Global)
    /// Department is a special principals,
    /// While Tenant &amp; Global are 'regular' users
    /// </summary>
    public const string PrincipalLevel = "principalLevel";

    /// <summary>
    /// The ID of the entity that the Principal belongs to.
    /// Depending on the PrincipalLevel, refers to either a 
    /// patient, a department, a tenant, or nothing (for site-level principal)
    /// </summary>
    public const string EntityId = "EntityId";

    ///<summary>A claim that specifies the full name of an entity</summary>
    public const string FullName = "fullname";

    ///<summary>A claim that specifies the job title of an entity</summary>
    public const string JobTitle = "jobtitle";

    ///<summary>A claim that specifies the email of an entity</summary>
    public const string Email = "email";

    ///<summary>A claim that specifies the phone number of an entity</summary>
    public const string Phone = "phone";

    ///<summary>A claim that specifies the configuration/customizations of an entity</summary>
    public const string Configuration = "configuration";
  }
}

namespace PskOnline.Client.Api
{
  using System;
  using System.Collections.Generic;
  using System.IdentityModel.Tokens.Jwt;
  using System.Security;
  using System.Security.Claims;
  using System.Threading.Tasks;

  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Inspection;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Client.Api.Tenant;

  public interface IApiClient : IDisposable
  {
    /// <summary>
    /// Signs in using the provided credentials
    /// </summary>
    /// <returns></returns>
    Task<TokenHolder> SignInWithUserPasswordAsync(string login, string password);

    /// <summary>
    /// Signs in using the provided credentials
    /// </summary>
    /// <returns></returns>
    Task<TokenHolder> SignInWithUserPasswordAsync(SecureString login, SecureString password);

    /// <summary>
    /// Signs in using a workplace credentials
    /// </summary>
    /// <returns></returns>
    Task<TokenHolder> SignInWithWorkplaceCredentialsAsync(string clientId, string clientCredentials, string scopes);

    /// <summary>
    /// Attempts to refresh the current access token using the available 
    /// authentication information (a refresh token or credentials,
    /// if these are cached)
    /// </summary>
    /// <returns></returns>
    Task RefreshToken();

    /// <summary>
    /// Retrieves the current ID Token (in JWT format)
    /// </summary>
    /// <returns></returns>
    JwtSecurityToken GetIdToken();

    Task<Guid> PostTenantAsync(TenantCreateDto tenant);

    Task<TenantDto> GetTenantAsync(string tenantId);

    Task<TenantSharedInfoDto> GetTenantSharedInfoAsync(string tenantId);

    Task<TenantOperationsSummaryDto> GetTenantOperationsSummaryAsync(string tenantId);

    Task<IEnumerable<BranchOfficeDto>> GetBranchOfficesAsync();

    /// <summary>
    /// returns information about branch office available to the current user
    /// depending on user attributes and permissions, this routine may return
    /// zero, one or several branch offices
    /// </summary>
    /// <param name="branchOfficeId"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException">thrown if user is not allowed to read the specified branch office</exception>
    Task<BranchOfficeDto> GetBranchOfficeAsync(Guid branchOfficeId);

    Task<BranchOfficeDto> GetBranchOfficeAsync(string branchOfficeId);

    Task<Guid> PostBranchOfficeAsync(BranchOfficeDto branchOffice);

    Task PutBranchOfficeAsync(BranchOfficeDto branchOffice);

    /// <summary>
    /// returns information about departments available to the currently logged in user
    /// depending on user attributes and permissions this routine may return
    /// zero, one or several departments
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException">thrown if user is not allowed to read departments in the specified branch office</exception>
    Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync();

    /// <summary>
    /// returns information about the specified department
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException">thrown if user is not allowed to read departments in the specified department</exception>
    Task<DepartmentDto> GetDepartmentAsync(Guid departmentId);

    Task<DepartmentDto> GetDepartmentAsync(string departmentId);

    Task<Guid> PostDepartmentAsync(DepartmentDto department);

    Task PutDepartmentAsync(DepartmentDto department);

    Task DeleteDepartmentAsync(Guid departmentId);

    Task DeleteDepartmentAsync(string departmentId);

    Task<Guid> PostPositionAsync(PositionDto position);

    Task<PositionDto> GetPositionAsync(Guid positionId);

    Task<PositionDto> GetPositionAsync(string positionId);

    Task<IEnumerable<PositionDto>> GetPositionsAsync();

    Task PutPositionAsync(PositionDto position);

    Task DeletePositionAsync(Guid positionid);

    Task DeletePositionAsync(string positionid);

    Task<Guid> PostEmployeeAsync(EmployeePostDto employee);

    Task<EmployeeDto> GetEmployeeAsync(Guid employeeId);

    Task<IEnumerable<EmployeeDto>> GetEmployeesAsync();

    Task<EmployeeDto> GetEmployeeAsync(string employeeId);

    Task PutEmployeeAsync(EmployeePostDto employee);

    Task DeleteEmployeeAsync(Guid employeeId);

    Task DeleteEmployeeAsync(string employeeId);

    /// <summary>
    /// returns information about the currently authenticated user
    /// </summary>
    /// <returns></returns>
    /// <remarks>this call would fail if the token used for authentication is issued to a 'department workplace' instead of an individual user</remarks>
    Task<UserDto> GetCurrentUserAsync();

    /// <summary>
    /// returns a collection of employees in the specified department
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException">thrown if user is not allowed to read users in the specified department</exception>
    Task<IEnumerable<EmployeeDto>> GetDepartmentEmployeesAsync(string departmentId);

    /// <summary>
    /// Uploads an inspection to the server.
    /// Handles 'Found' response, returning the ID of the found inspection.
    /// </summary>
    /// <param name="inspection"></param>
    /// <returns></returns>
    Task<Guid> PostInspectionAsync(InspectionPostDto inspection);

    Task<InspectionDto> GetInspectionAsync(Guid inspectionId);

    Task<InspectionDto> GetInspectionAsync(string inspectionId);

    /// <summary>
    /// Completes the specified inspection and fetches the descriptors
    /// of the summaries generated by the available plugins on the server.
    /// To fetch the results JSONs, as needed, use
    /// <see cref="IApiClient.HttpGetAsJsonFromRelativeUriAsync{T}(string)"/>
    /// </summary>
    /// <param name="inspectionCompletion"></param>
    /// <returns>An object containing an array of results descriptors</returns>
    Task<InspectionCompleteResponseDto> CompleteInspectionAsync(InspectionCompleteDto inspectionCompletion);

    /// <summary>
    /// Uploads a test to the server.
    /// Handles 'Found' response, returning the ID of the found test.
    /// </summary>
    /// <param name="test"></param>
    /// <returns></returns>
    Task<Guid> PostTestAsync(TestPostDto test);

    Task<Guid> PostRoleAsync(RoleDto role);

    Task<IEnumerable<RoleDto>> GetRolesAsync();

    Task<RoleDto> GetRoleAsync(string roleId);

    Task<RoleDto> GetRoleAsync(Guid roleId);

    Task PutRoleAsync(RoleDto role);

    Task DeleteRoleAsync(string roleId);

    Task<IEnumerable<PermissionDto>> GetPermissionsAsync();

    Task<UserDto> GetUserByUsernameAsync(string username);

    Task<IEnumerable<UserDto>> GetUsersAsync();

    Task<UserDto> GetUserAsync(Guid userId);

    Task<UserDto> GetUserAsync(string userId);

    Task PutUserAsync(UserDto user);

    /// <summary>
    /// Performs an HTTP GET request on the specified Uri relative to the 
    /// BaseAddress. The Uri must not start with the slash ('/') character.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="relativeUri"></param>
    /// <returns></returns>
    Task<T> HttpGetAsJsonFromRelativeUriAsync<T>(string endpointUri);
  }
}

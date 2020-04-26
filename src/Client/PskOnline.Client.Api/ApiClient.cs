namespace PskOnline.Client.Api
{
  using Microsoft.Extensions.Logging;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Inspection;
  using PskOnline.Client.Api.Models;
  using PskOnline.Client.Api.OpenId;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Client.Api.Tenant;
  using System;
  using System.Collections.Generic;
  using System.IdentityModel.Tokens.Jwt;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Security;
  using System.Threading.Tasks;

  /// <summary>
  /// Implements the client for PskOnline REST API
  /// </summary>
  /// <remarks>The HTTP client used to construct the API client, must not automatically follow redirects!</remarks>
  public class ApiClient : IApiClient
  {
    private ILogger _logger;

    private HttpClient _httpClient;

    private ITokenRenewalHandler _openIdRenewalHandler;

    private string BaseAddress => _httpClient.BaseAddress.AbsoluteUri;

    const string usersEndpoint = "api/account/users";
    const string usersByNameEndpoint = "api/account/users/username";
    const string rolesEndpoint = "api/account/roles";
    const string permissionsEndpoint = "api/account/permissions";
    const string branchOfficesEndpoint = "api/BranchOffice";
    const string departmentsEndpoint = "api/Department";
    const string positionsEndpoint = "api/Position";
    const string tenantsEndpoint = "api/Tenant";
    const string employeesEndpoint = "api/Employee";
    const string testsEndpoint = "api/Test";
    const string inspectionsEndpoint = "api/Inspection";

    /// <summary>
    /// Creates API client without authentication information.
    /// Authentication should be performed with <see cref="SignInWithUserPasswordAsync(SecureString, SecureString)"/> 
    /// or with <see cref="SignInWithWorkplaceCredentialsAsync"/>
    /// </summary>
    /// <remarks>The HTTP client used to construct the API client, must not automatically follow redirects!</remarks>
    /// <param name="client"></param>
    /// <param name="logger"></param>
    public ApiClient(HttpClient client, ILogger logger)
    {
      _logger = logger;
      _httpClient = client;
      _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Enables tracing of server responses for debugging purposes
    /// </summary>
    public bool TraceResponseOnException { get; set; }

    /// <summary>
    /// Creates API client with a set of tokens for authentication.
    /// If refresh_token is provided, it will be used for token renewal.
    /// </summary>
    /// <remarks>The HTTP client used to construct the API client, must not automatically follow redirects!</remarks>
    /// <param name="client"></param>
    /// <param name="tokens">a set of tokens to use for authenticaton and renewal</param>
    /// <param name="logger"></param>
    public ApiClient(HttpClient client, TokenHolder tokens, ILogger logger) : this(client, logger)
    {
      _openIdRenewalHandler = new OpenIdRenewalHandler(client, tokens, null, _logger);
    }

    /// <summary>
    /// Creates API client
    /// </summary>
    /// <remarks>The HTTP client used to construct the API client, must not automatically follow redirects!</remarks>
    /// <param name="client"></param>
    /// <param name="tokenRenewalHandler"></param>
    /// <param name="logger"></param>
    public ApiClient(HttpClient client, ITokenRenewalHandler tokenRenewalHandler, ILogger logger) : this(client, logger)
    {
      _openIdRenewalHandler = tokenRenewalHandler;
    }

    /// <summary>
    /// Retuns the ID token, if available (the must authenticate to obtain the ID token)
    /// </summary>
    /// <returns></returns>
    public JwtSecurityToken GetIdToken()
    {
      return _openIdRenewalHandler.GetIdToken();
    }

    /// <summary>
    /// Signs in using the provided credentials
    /// </summary>
    /// <returns></returns>
    public Task<TokenHolder> SignInWithUserPasswordAsync(string username, string password)
    {
      return SignInWithUserPasswordAsync(username.ToSecureString(), password.ToSecureString());
    }

    /// <summary>
    /// Signs in using the provided credentials
    /// </summary>
    /// <returns></returns>
    public async Task<TokenHolder> SignInWithUserPasswordAsync(SecureString login, SecureString password)
    {
      var loginHandler = new ResourceOwnerPasswordHandler(
        _httpClient, login, password);
      var tokens = await loginHandler.AuthenticateAsync();

      _openIdRenewalHandler = new OpenIdRenewalHandler(_httpClient, tokens, loginHandler, _logger);

      _logger.LogInformation("Successfully signed in with user credentials");
      return tokens;
    }

    /// <summary>
    /// Signs in using a workplace token
    /// </summary>
    /// <returns></returns>
    public async Task<TokenHolder> SignInWithWorkplaceCredentialsAsync(string clientId, string clientSecret, string scopes)
    {
      var loginHandler = new ClientCredentialsHandler(
        _httpClient, clientId.ToSecureString(), clientSecret.ToSecureString(), scopes);
      var tokens = await loginHandler.AuthenticateAsync();
      _openIdRenewalHandler = new OpenIdRenewalHandler(_httpClient, tokens, loginHandler, _logger);

      _logger.LogInformation("Successfully signed in with client (workplace) credentials");
      return tokens;
    }

    /// <summary>
    /// Attempts to refresh tokens using the available authentication information
    /// (a refresh token or credentials, if these are cached)
    /// </summary>
    /// <returns></returns>
    public async Task RefreshToken()
    {
      await _openIdRenewalHandler.RefereshAuthenticationAsync();
    }

    private async Task RefreshTokenIfNeeded()
    {
      await _openIdRenewalHandler.RefereshAuthenticationIfNeededAsync();
    }

    private async Task<T> ExecuteWithAuthenticationAndReadAsJson<T>(Func<Task<HttpResponseMessage>> httpAction)
    {
      // request a new token in case the current one has expired (or is about to expire)
      await RefreshTokenIfNeeded();
      HttpResponseMessage response = await httpAction();
      if (response.IsSuccessStatusCode)
      {
        if (response.Content != null && response.Content.IsJsonContent() )
        {
          try
          {
            return await response.Content.ReadAsJsonAsync<T>();
          }
          catch (Exception ex) when (TraceResponseOnException)
          {
            var text = await response.Content.ReadAsStringAsync();
            var nl = Environment.NewLine;
            throw new Newtonsoft.Json.JsonSerializationException(
              $"{ex.Message}{nl}Server response (length = {text.Length}):{nl}{text}{nl}",
              ex);
          }
        }
        return default(T);
      }
      if (response.StatusCode == HttpStatusCode.Unauthorized)
      {
        _logger.LogInformation("The server returned 'Unauthorized' status. Trying to refresh the access token.");
        // atttempt to referesh the token explicitly
        await RefreshToken();
        response = await httpAction();
        if (response.IsSuccessStatusCode)
        {
          return await response.Content.ReadAsJsonAsync<T>();
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          LogStillUnauthorizedAfterRefreshingAccessToken();
          throw await CreateException(response);
        }
        throw await CreateException(response);
      }
      throw await CreateException(response);
    }

    private void LogStillUnauthorizedAfterRefreshingAccessToken()
    {
      var identity = GetIdToken();
      _logger.LogWarning(
        "The server returned 'Unauthorized' status right after returning a new token. " +
        $" The current identity is: {identity?.Subject}, {identity?.ToString()}."
        );
    }

    private async Task<HttpResponseMessage> ExecuteWithAuthentication(Func<Task<HttpResponseMessage>> function)
    {
      // request a new token in case the current one has expired
      // (or is about to expire), based on the stored validity period
      await RefreshTokenIfNeeded();
      HttpResponseMessage response = await function();
      if (response.IsSuccessStatusCode)
      {
        return response;
      }
      if (response.StatusCode == HttpStatusCode.Unauthorized)
      {
        _logger.LogInformation("The server returned 'Unauthorized' status. Trying to refresh the access token.");
        // clear the invalid token
        await _openIdRenewalHandler.RefereshAuthenticationAsync();
        response = await function();
        if (response.IsSuccessStatusCode)
        {
          return response;
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          LogStillUnauthorizedAfterRefreshingAccessToken();
          throw await CreateException(response);
        }
        throw await CreateException(response);
      }
      throw await CreateException(response);
    }

    private Task<Exception> CreateException(HttpResponseMessage response)
    {
      return ExceptionHelper.CreateExceptionAsync(response);
    }

    private async Task<IEnumerable<T>> GetAllEntitiesAsync<T>(string endpoint) where T : class
    {
      return await ExecuteWithAuthenticationAndReadAsJson<IEnumerable<T>>(
        () => _httpClient.GetAsync(BaseAddress + endpoint));
    }

    private async Task<T> GetEntityAsync<T>(string endpoint, string id) where T : class
    {
      return await ExecuteWithAuthenticationAndReadAsJson<T>(
        () => _httpClient.GetAsync(BaseAddress + endpoint + "/" + id));
    }

    private async Task<Guid> PostGuidEntityAsync(string endpoint, object entity)
    {
      try
      {
        var createdDto = 
          await ExecuteWithAuthenticationAndReadAsJson<CreatedWithGuidDto>(
            () => _httpClient.PostAsJsonAsync(BaseAddress + endpoint, entity));
        return createdDto.Id;
      }
      catch( Exception ex )
      {
        if (ex is ItemAlreadyExistsException found)
        {
          if (found.Id.HasValue)
          {
            return found.Id.Value;
          }
        }
        // this is an unexpected condition
        throw;
      }
    }

    private Task PutEntityAsync(string endpoint, string id, object entity)
    {
      return ExecuteWithAuthentication(
        () => _httpClient.PutAsJsonAsync(BaseAddress + endpoint + "/" + id, entity));
    }

    private Task DeleteEntityAsync(string endpoint, string entityId)
    {
      return ExecuteWithAuthentication(() =>
        _httpClient.DeleteAsync(BaseAddress + endpoint + "/" + entityId)
        );
    }

    #region methods related to Tenants
    /// <summary>
    /// Creates a new tenant on the server. Check the server documentation for the required permissions.
    /// </summary>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public async Task<Guid> PostTenantAsync(TenantCreateDto tenant)
    {
      var newId = await PostGuidEntityAsync(tenantsEndpoint, tenant);
      tenant.TenantDetails.Id = newId.ToString();
      return newId;
    }

    /// <summary>
    /// Retrieves the information about the specified tenant.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public Task<TenantDto> GetTenantAsync(string tenantId)
    {
      return GetEntityAsync<TenantDto>(tenantsEndpoint, tenantId);
    }

    /// <summary>
    /// Retrieves the information about the specified tenant, available to all users within the tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public Task<TenantSharedInfoDto> GetTenantSharedInfoAsync(string tenantId)
    {
      return GetEntityAsync<TenantSharedInfoDto>(tenantsEndpoint + "/" + tenantId, "shared-info");
    }

    public Task<TenantOperationsSummaryDto> GetTenantOperationsSummaryAsync(string tenantId)
    {
      return GetEntityAsync<TenantOperationsSummaryDto>(tenantsEndpoint + "/" + tenantId, "operations-summary");
    }
    #endregion

    #region methods related to Branch Offices
    public Task<BranchOfficeDto> GetBranchOfficeAsync(Guid branchOfficeId)
    {
      return GetBranchOfficeAsync(branchOfficeId.ToString());
    }

    public Task<BranchOfficeDto> GetBranchOfficeAsync(string branchOfficeId)
    {
      return GetEntityAsync<BranchOfficeDto>(branchOfficesEndpoint, branchOfficeId);
    }

    public async Task<Guid> PostBranchOfficeAsync(BranchOfficeDto branchOffice)
    {
      var newId = await PostGuidEntityAsync(branchOfficesEndpoint, branchOffice);
      branchOffice.Id = newId.ToString();
      return newId;
    }

    public Task PutBranchOfficeAsync(BranchOfficeDto branchOffice)
    {
      return PutEntityAsync(branchOfficesEndpoint, branchOffice.Id, branchOffice);
    }

    public Task<IEnumerable<BranchOfficeDto>> GetBranchOfficesAsync()
    {
      return GetAllEntitiesAsync<BranchOfficeDto>(branchOfficesEndpoint);
    }
    #endregion

    #region methods related to Departments
    public Task<UserDto> GetCurrentUserAsync()
    {
      return GetEntityAsync<UserDto>(usersEndpoint, "me");
    }

    public Task<DepartmentDto> GetDepartmentAsync(Guid departmentId)
    {
      return GetDepartmentAsync(departmentId.ToString());
    }

    public Task<DepartmentDto> GetDepartmentAsync(string departmentId)
    {
      return GetEntityAsync<DepartmentDto>(departmentsEndpoint, departmentId);
    }

    public async Task<Guid> PostDepartmentAsync(DepartmentDto department)
    {
      var newId = await PostGuidEntityAsync(departmentsEndpoint, department);
      department.Id = newId.ToString();
      return newId;
    }

    public Task PutDepartmentAsync(DepartmentDto department)
    {
      return PutEntityAsync(departmentsEndpoint, department.Id, department);
    }

    public Task DeleteDepartmentAsync(Guid departmentId)
    {
      return DeleteDepartmentAsync(departmentId.ToString());
    }

    public Task DeleteDepartmentAsync(string departmentId)
    {
      return DeleteEntityAsync(departmentsEndpoint, departmentId);
    }

    public Task<IEnumerable<EmployeeDto>> GetDepartmentEmployeesAsync(string departmentId)
    {
      return GetAllEntitiesAsync<EmployeeDto>(departmentsEndpoint + "/" + departmentId + "/employees");
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync()
    {
      return await GetAllEntitiesAsync<DepartmentDto>(departmentsEndpoint);
    }
    #endregion

    #region methods related to Positions
    public async Task<Guid> PostPositionAsync(PositionDto position)
    {
      var newId = await PostGuidEntityAsync(positionsEndpoint, position);
      position.Id = newId.ToString();
      return newId;
    }

    public Task<PositionDto> GetPositionAsync(Guid positionId)
    {
      return GetPositionAsync(positionId.ToString());
    }

    public Task<PositionDto> GetPositionAsync(string positionId)
    {
      return GetEntityAsync<PositionDto>(positionsEndpoint, positionId);
    }

    public Task<IEnumerable<PositionDto>> GetPositionsAsync()
    {
      return GetAllEntitiesAsync<PositionDto>(positionsEndpoint);
    }

    public Task PutPositionAsync(PositionDto position)
    {
      return PutEntityAsync(positionsEndpoint, position.Id, position);
    }

    public Task DeletePositionAsync(Guid positionId)
    {
      return DeletePositionAsync(positionId.ToString());
    }

    public Task DeletePositionAsync(string positionId)
    {
      return DeleteEntityAsync(positionsEndpoint, positionId);
    }
    #endregion

    #region methods related to Employees
    public async Task<Guid> PostEmployeeAsync(EmployeePostDto employee)
    {
      var newId = await PostGuidEntityAsync(employeesEndpoint, employee);
      employee.Id = newId.ToString();
      return newId;
    }

    public Task<EmployeeDto> GetEmployeeAsync(Guid employeeId)
    {
      return GetEmployeeAsync(employeeId.ToString());
    }

    public Task<IEnumerable<EmployeeDto>> GetEmployeesAsync()
    {
      return GetAllEntitiesAsync<EmployeeDto>(employeesEndpoint);
    }

    public Task<EmployeeDto> GetEmployeeAsync(string employeeId)
    {
      return GetEntityAsync<EmployeeDto>(employeesEndpoint, employeeId);
    }

    public Task PutEmployeeAsync(EmployeePostDto employee)
    {
      return PutEntityAsync(employeesEndpoint, employee.Id, employee);
    }

    public Task DeleteEmployeeAsync(Guid employeeId)
    {
      return DeleteEmployeeAsync(employeeId.ToString());
    }

    public Task DeleteEmployeeAsync(string employeeId)
    {
      return DeleteEntityAsync(employeesEndpoint, employeeId);
    }
    #endregion

    #region Methods related to inspections and tests
    public async Task<Guid> PostInspectionAsync(InspectionPostDto inspection)
    {
      var newId = await PostGuidEntityAsync(inspectionsEndpoint, inspection);
      inspection.Id = newId.ToString();
      return newId;
    }

    public Task<InspectionDto> GetInspectionAsync(Guid inspectionId)
    {
      return GetInspectionAsync(inspectionId.ToString());
    }

    public Task<InspectionDto> GetInspectionAsync(string inspectionId)
    {
      return GetEntityAsync<InspectionDto>(inspectionsEndpoint, inspectionId);
    }

    public async Task<InspectionCompleteResponseDto> CompleteInspectionAsync(InspectionCompleteDto completeDto)
    {
      var response = await ExecuteWithAuthentication(() => {
        var url = inspectionsEndpoint + "/" + completeDto.Id.ToString() + "/complete";
        return _httpClient.PutAsJsonAsync(BaseAddress + url, completeDto);
        });

      var dto = await response.Content.ReadAsJsonAsync<InspectionCompleteResponseDto>();
      var baseLength = BaseAddress.Length;

      var pluginResults = dto.PluginResults.Select(r => new PluginResultDescriptorDto
      {
        PluginType = r.PluginType,
        ResultsUrl = r.ResultsUrl.StartsWith(BaseAddress) ? r.ResultsUrl.Remove(0, baseLength) : r.ResultsUrl
      }).ToArray();

      return new InspectionCompleteResponseDto {
        PluginResults = pluginResults
      };
    }

    public async Task<Guid> PostTestAsync(TestPostDto test)
    {
      var newId = await PostGuidEntityAsync(testsEndpoint, test);
      test.Id = newId.ToString();
      return newId;
    }
    #endregion

    #region Methods related to users, roles and permissions
    public Task<IEnumerable<UserDto>> GetUsersAsync()
    {
      return GetAllEntitiesAsync<UserDto>(usersEndpoint);
    }

    public Task<UserDto> GetUserAsync(Guid userId)
    {
      return GetUserAsync(userId.ToString());
    }

    public Task<UserDto> GetUserAsync(string userId)
    {
      return GetEntityAsync<UserDto>(usersEndpoint, userId);
    }

    public Task PutUserAsync(UserDto user)
    {
      return PutEntityAsync(usersEndpoint, user.Id, user);
    }

    public Task<UserDto> GetUserByUsernameAsync(string username)
    {
      return GetEntityAsync<UserDto>(usersByNameEndpoint, username);
    }

    public async Task<IEnumerable<RoleDto>> GetRolesAsync()
    {
      return await GetAllEntitiesAsync<RoleDto>(rolesEndpoint);
    }

    public async Task<Guid> PostRoleAsync(RoleDto role)
    {
      var newId = await PostGuidEntityAsync(rolesEndpoint, role);
      role.Id = newId.ToString();
      return newId;
    }

    public Task PutRoleAsync(RoleDto role)
    {
      return PutEntityAsync(rolesEndpoint, role.Id, role);
    }

    public Task DeleteRoleAsync(string roleId)
    {
      return DeleteEntityAsync(rolesEndpoint, roleId);
    }

    public Task<RoleDto> GetRoleAsync(Guid roleId)
    {
      return GetRoleAsync(roleId.ToString());
    }

    public Task<RoleDto> GetRoleAsync(string roleId)
    {
      return GetEntityAsync<RoleDto>(rolesEndpoint, roleId);
    }

    public Task<IEnumerable<PermissionDto>> GetPermissionsAsync()
    {
      return GetAllEntitiesAsync<PermissionDto>(permissionsEndpoint);
    }

    #endregion
    /// <summary>
    /// Executes an HTTP GET request on the specified relative URL.
    /// Upon success, deserializes the content of the server's response
    /// as a JSON containin an instance of the type <typeparamref name="T"/>
    /// and returns the deserialized object.
    /// </summary>
    /// <typeparam name="T">The type to convert the response to</typeparam>
    /// <param name="relativeUri">For example: /api/plugins/psa-demo/inspection/{inspection-guid}</param>
    /// <returns></returns>
    public Task<T> HttpGetAsJsonFromRelativeUriAsync<T>(string relativeUri)
    {
      return ExecuteWithAuthenticationAndReadAsJson<T>(
        () => _httpClient.GetAsync(BaseAddress + relativeUri));
    }

    public void Dispose()
    {
    }
  }
}

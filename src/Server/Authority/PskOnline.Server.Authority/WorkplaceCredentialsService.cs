namespace PskOnline.Server.Authority
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Cryptography;
  using System.Text;
  using System.Threading.Tasks;
  using AutoMapper;
  using Microsoft.EntityFrameworkCore;
  using Newtonsoft.Json;
  using OpenIddict.Abstractions;
  using PskOnline.Server.Authority.API;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.EntityFramework;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;

  public class WorkplaceCredentialsService : IWorkplaceCredentialsService
  {
    private readonly IOpenIddictApplicationManager _oidcAppManager;
    private readonly AuthorityDbContext _authorityDbContext;
    private readonly IMapper _mapper;
    private readonly RNGCryptoServiceProvider _rngCsp;
    private readonly ITenantIdProvider _tenantIdProvider;

    public WorkplaceCredentialsService(
      IOpenIddictApplicationManager oidcAppManager,
      AuthorityDbContext authorityDbContext,
      AutoMapperConfig autoMapperConfig,
      ITenantIdProvider tenantIdProvider
      )
    {
      _rngCsp = new RNGCryptoServiceProvider();

      _oidcAppManager = oidcAppManager;
      _tenantIdProvider = tenantIdProvider;
      _authorityDbContext = authorityDbContext;
      _mapper = autoMapperConfig.CreateMapper();
    }

    public async Task<WorkplaceCredentialsDto> CreateWorkplaceAsync(WorkplaceDescriptorDto workplaceDescriptor)
    {
      var credentials = CreateCredentials(workplaceDescriptor);

      var permissions = new List<string>(8);
      permissions.AddRange(workplaceDescriptor.Scopes.Select(s => "scp:" + s));
      permissions.AddRange(new[] {
        OpenIddictConstants.Permissions.Endpoints.Token,
        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials
      });

      var application = new PskApplication
      {
        ClientId = credentials.ClientId,
        DisplayName = workplaceDescriptor.DisplayName,
        Permissions = JsonConvert.SerializeObject(permissions),
        TenantId = workplaceDescriptor.TenantId,
        BranchOfficeId = workplaceDescriptor.BranchOfficeId,
        DepartmentId = workplaceDescriptor.DepartmentId,
        ApplicationType = "workplace_" + workplaceDescriptor.WorkplaceType
      };

      await _oidcAppManager.CreateAsync(application, credentials.ClientSecret);

      return credentials;
    }

    /// <summary>
    /// returns an evenly-distributed random value not greater than max
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    private byte GetRandomValue(byte max)
    {
      var array = new byte[1];
      do
      {
        _rngCsp.GetBytes(array);
      }
      while (array[0] >= (byte.MaxValue - byte.MaxValue % max));

      return (byte)(array[0] % max);
    }

    private string GetRandomStringInBase52Alphabeth(int length)
    {
      const string alphabeth = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
      var sb = new StringBuilder(length * 2);
      for (var i = 0; i < length; ++i)
      {
        sb.Append(alphabeth[GetRandomValue(51)]);
      }
      return sb.ToString();
    }

    public WorkplaceCredentialsDto CreateCredentials(WorkplaceDescriptorDto descriptor)
    {
      var userTenant = _tenantIdProvider.GetTenantId().ToString();
      if (userTenant != descriptor.TenantId)
      {
        throw new BadRequestException("Bad tenant id specified");
      }

      var clientId = descriptor.WorkplaceType + GetRandomStringInBase52Alphabeth(16);
      var clientSecret = CreateSecret();
      return new WorkplaceCredentialsDto
      {
        ClientId = clientId,
        ClientSecret = clientSecret
      };
    }

    private string CreateSecret()
    {
      var secretLength = 17 + GetRandomValue(6);
      return GetRandomStringInBase52Alphabeth(secretLength);
    }

    /// <summary>
    /// Returns a collection of workplaces for the specified department
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<WorkplaceDto>> GetForDepartmentAsync(Guid departmentId)
    {
      var userTenantId = _tenantIdProvider.GetTenantId().ToString();
      var deptId = departmentId.ToString();
      // remember to limit the output to current tenant only
      var apps = await _authorityDbContext
        .Set<PskApplication>()
        .Where(a => a.DepartmentId == deptId && a.TenantId == userTenantId)
        .ToListAsync();

      return _mapper.Map<List<WorkplaceDto>>(apps);
    }


    public async Task<string> UpdateWorkplaceSecretAsync(string clientId)
    {
      var userTenantId = _tenantIdProvider.GetTenantId().ToString();

      // remember to limit the search to current tenant only
      var application = _authorityDbContext
        .Set<PskApplication>()
        .FirstOrDefault(a => a.ClientId == clientId && a.TenantId == userTenantId);

      if (application != null)
      {
        var newSecret = CreateSecret();
        await _oidcAppManager.UpdateAsync(application, newSecret);
        return newSecret;
      }
      else
      {
        throw new ItemNotFoundException(clientId, "Workplace");
      }
    }

    public async Task DeleteAsync(string clientId)
    {
      var userTenantId = _tenantIdProvider.GetTenantId().ToString();

      var app = (await _oidcAppManager.FindByClientIdAsync(clientId) as PskApplication);

      if (app != null && app.TenantId == userTenantId)
      {
        await _oidcAppManager.DeleteAsync(app);
        return;
      }

      throw new ItemNotFoundException(clientId, "Workplace");
    }
  }
}

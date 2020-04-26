namespace PskOnline.Server.DAL.Multitenancy
{
  using System;
  using System.Threading.Tasks;

  using PskOnline.Server.Authority.API;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Shared.Permissions;
  using PskOnline.Server.Shared.Roles;

  public class TenantRolesCreator
  {
    static TenantRolesCreator()
    {
      var config = new AutoMapper.MapperConfiguration(cfg => {
        cfg.CreateMap<IApplicationPermission, PermissionDto>()
          .ForMember(m => m.GroupName, a => a.Ignore())
          .ForMember(m => m.Description, a => a.Ignore())
          .ForMember(m => m.Name, a => a.Ignore());
        cfg.CreateMap<RoleSpecification, RoleDto>();
        cfg.CreateMap<RoleDto, RoleSpecification>()
          .ForSourceMember(m => m.UsersCount, a => a.Ignore());

      });

      MapperInstance = config.CreateMapper();
    }
    private static readonly AutoMapper.IMapper MapperInstance;

    private readonly IRoleService _roleService;

    public TenantRolesCreator(IRoleService roleService)
    {
      _roleService = roleService;
    }

    public async Task<Guid> CreateDefaultRolesForTenantAsync(Guid tenantId)
    {
      var tenantAdminRoleDef = DefaultTenantRoles.TenantAdministratorTemplate;
      tenantAdminRoleDef.Id = Guid.NewGuid();
      var tenantAdminRoleDefDto = MapperInstance.Map<RoleDto>(tenantAdminRoleDef);
      await _roleService.CreateRoleAsync(tenantAdminRoleDefDto, tenantId);

      // we don't really care about IDs of these roles
      var tenantRoleTemplates = DefaultTenantRoles.GetStandardTenantNonAdminRoleTemplates();
      foreach (var roleDef in tenantRoleTemplates)
      {
        var roleDefDto = MapperInstance.Map<RoleDto>(roleDef);
        roleDefDto.Id = Guid.NewGuid().ToString();
        await _roleService.CreateRoleAsync(roleDefDto, tenantId);
      }

      return tenantAdminRoleDef.Id;
    }
  }
}

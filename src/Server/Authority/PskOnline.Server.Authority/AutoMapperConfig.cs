namespace PskOnline.Server.Authority
{
  using Microsoft.AspNetCore.Identity;
  using Newtonsoft.Json.Linq;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Permissions;
  using System;
  using System.Linq;

  public class AutoMapperConfig : AutoMapper.MapperConfiguration
  {
    public AutoMapperConfig() : base(cfg => cfg.AddProfile<AutoMapperProfile>())
    {
    }
  }

  public class AutoMapperProfile : AutoMapper.Profile
  {
    public AutoMapperProfile()
    {
      CreateMap<ApplicationUser, UserDto>()
             .ForMember(d => d.Roles, map => map.Ignore());
      CreateMap<UserDto, ApplicationUser>()
          .ForMember(d => d.Roles, map => map.Ignore());

      CreateMap<ApplicationUser, UserEditDto>()
          .ForMember(d => d.Roles, map => map.Ignore());

      CreateMap<UserEditDto, UserEditDto>();

      CreateMap<UserDto, UserEditDto>().ReverseMap();

      CreateMap<UserEditDto, ApplicationUser>()
          .ForMember(d => d.Roles, map => map.Ignore());

      CreateMap<ApplicationRole, UserRoleInfo>().ReverseMap();

      CreateMap<UserPatchDto, ApplicationUser>().ReverseMap();

      CreateMap<ApplicationRole, RoleDto>()
          .ForMember(d => d.Permissions, map => map.MapFrom(s => s.Claims))
          .ForMember(d => d.UsersCount, map => map.ResolveUsing(s => s.Users?.Count ?? 0))
          .ReverseMap();
      CreateMap<RoleDto, ApplicationRole>();

      CreateMap<ApplicationRole, RoleInTenantDto>()
          .ForMember(d => d.Permissions, map => map.MapFrom(s => s.Claims))
          .ForMember(d => d.UsersCount, map => map.ResolveUsing(s => s.Users?.Count ?? 0))
          .ReverseMap();

      CreateMap<ApplicationPermission, PermissionDto>()
          .ReverseMap();

      CreateMap<IdentityRoleClaim<Guid>, PermissionDto>()
          .ConvertUsing<ClaimValueToPermissionDtoConverter>();

      // source => destination
      CreateMap<PskApplication, WorkplaceDto>()
          .ForMember(d => d.ClientId, map => map.MapFrom(s => s.ClientId))
          .ForMember(d => d.Scopes, map => map.ResolveUsing( s =>
          {
            var scopes = "";
            var perms = s.Permissions.TrimStart('[').TrimEnd(']').Split(',')
              .Select(p => p.TrimEnd('"', ' ').TrimStart('"', ' ')).ToList();
            return perms
              .Where(p => p.StartsWith("scp:"))
              .Select(p => p.Substring(4, p.Length - 4))
              .Aggregate(scopes, (r, p) => r.Length > 0 ? (r += " " + p) : (r += p));

          }))
          .ForMember(d => d.WorkplaceType, map => map.ResolveUsing(s => 
          {
            var prefix = "workplace_";
            return s.ApplicationType.Substring(prefix.Length, s.ApplicationType.Length - prefix.Length);
          }));

    }
  }
}

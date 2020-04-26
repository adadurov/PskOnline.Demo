namespace PskOnline.Server.Service.ViewModels
{
  using System;
  using Microsoft.AspNetCore.Identity;

  using AutoMapper;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.DAL.Inspections;

  public class AutoMapperProfile : Profile
  {
    public AutoMapperProfile()
    {
      CreateMap<UserEditDto, UserEditDto>();

      CreateMap<UserDto, UserEditDto>().ReverseMap();

      CreateMap<TenantCreateAdminDto, UserDto>();

      CreateMap<IdentityRoleClaim<Guid>, ClaimDto>()
        .ForMember(d => d.Type, map => map.MapFrom(s => s.ClaimType))
        .ForMember(d => d.Value, map => map.MapFrom(s => s.ClaimValue))
        .ReverseMap();

      CreateMap<Tenant, TenantDto>()
        .ForMember(t => t.Slug, 
          map => map.AddTransform(s => s == null ? null : s.ToLower())
          );
      CreateMap<TenantDto, Tenant>();

      CreateMap<BranchOffice, BranchOfficeDto>()
        .ReverseMap();

      CreateMap<Department, DepartmentDto>()
        .ReverseMap();

      CreateMap<Employee, EmployeeDto>()
        .ReverseMap();

      CreateMap<Position, PositionDto>()
        .ReverseMap();

      CreateMap<Inspection, InspectionDto>()
        .ReverseMap();
      
      // this mapping is used for copying
      CreateMap<Inspection, Inspection>();
      CreateMap<Inspection, InspectionDto>().ReverseMap();
      CreateMap<Inspection, InspectionPostDto>().ReverseMap();

      CreateMap<PluginOutputDescriptor, PluginResultDescriptorDto>();

      CreateMap<Test, Test>();
      CreateMap<Test, TestDto>().ReverseMap();
      CreateMap<Test, TestPostDto>().ReverseMap();
    }
  }
}

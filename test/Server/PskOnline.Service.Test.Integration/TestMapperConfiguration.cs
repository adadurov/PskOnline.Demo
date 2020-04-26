namespace PskOnline.Service.Test.Integration
{
  using PskOnline.Client.Api.Authority;

  public static class TestMapperConfiguration
  {
    static TestMapperConfiguration()
    {
      Config = new AutoMapper.MapperConfiguration(cfg =>
      { 
        cfg.CreateMap<UserEditDto, UserDto>().ReverseMap();
        cfg.CreateMap<UserRoleInfo, RoleDto>().ReverseMap();
        cfg.CreateMap<UserEditDto, UserEditDto>();
        cfg.CreateMap<UserDto, UserDto>();
      });
    }

    public static AutoMapper.MapperConfiguration Config { get; private set; }
  }
}

namespace PskOnline.Service.Test.Integration.TestData
{

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
      CreateMap<PskOnline.Client.Api.Authority.UserEditDto, PskOnline.Client.Api.Authority.UserDto>()
        .ForSourceMember(d => d.NewPassword, map => map.Ignore())
        .ForSourceMember(d => d.CurrentPassword, map => map.Ignore());
      CreateMap<PskOnline.Client.Api.Authority.UserDto, PskOnline.Client.Api.Authority.UserEditDto>()
        .ForMember(d => d.CurrentPassword, map => map.Ignore())
        .ForMember(d => d.NewPassword, map => map.Ignore());

    }
  }
}

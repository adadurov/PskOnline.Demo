namespace PskOnline.Server.Plugins.RusHydro.Web
{
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;
  using PskOnline.Server.Plugins.RusHydro.Web.Dto;
  using System;

  public class AutoMapperConfig : AutoMapper.MapperConfiguration
  {
    static AutoMapperConfig()
    {
      Instance = new AutoMapperConfig();
    }

    public static AutoMapperConfig Instance { get; private set; }

    public AutoMapperConfig() : base(cfg => cfg.AddProfile<AutoMapperProfile>())
    {
    }
  }

  public class AutoMapperProfile : AutoMapper.Profile
  {
    public AutoMapperProfile()
    {
      CreateMap<SummaryDocument, PsaSummaryDto>();

      CreateMap<Employee, RushydroEmployeeDto>();

      CreateMap<ObjectModel.LSR_HrvFunctionalState, Dto.LSR_HrvFunctionalState>();

      CreateMap<PreShiftFinalConclusion, PsaFinalConclusionDto>();

      CreateMap<PreShiftHrvConclusion, HrvPreShiftConclusionDto>();

      CreateMap<PreShiftSvmrConclusion, SvmrPreShiftConclusionDto>();

      CreateMap<ObjectModel.PsaStatus, Dto.PsaStatus>();

      CreateMap<PsaReportDocument, PsaReportDto>();
    }
  }
}

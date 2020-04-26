using PskOnline.Math.Statistics;

namespace PskOnline.Methods.Processing.Logic
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using AutoMapper;

  public sealed class AutoMapperStatisticsProfile : Profile
  {
    public AutoMapperStatisticsProfile()
    {
      CreateMap< StatData, PskOnline.Methods.ObjectModel.Statistics.StatData>();
    }
  }
}

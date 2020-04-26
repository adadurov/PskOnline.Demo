namespace PskOnline.Server.Plugins.RusHydro.Web.Dto
{
  public class SvmrPreShiftConclusionDto : PsaMethodConclusionDto
  {
    public double IPN1 { get; set; }

    /// <summary>
    /// Среднее время реакции
    /// </summary>
    public double MeanResponseTimeMSec { get; set; }
  }
}
namespace PskOnline.Server.Plugins.RusHydro.Web.Dto
{
  public class HrvPreShiftConclusionDto : PsaMethodConclusionDto
  {
    public LSR_HrvFunctionalState LSR { get; set; }

    public string LSR_Text { get; set; }

    public double VSR { get; set; }

    /// <summary>
    /// Индекс напряжения
    /// </summary>
    public double IN { get; set; }

    public int StateMatrixRow { get; set; }

    public int StateMatrixCol { get; set; }

    /// <summary>
    /// Средняя ЧСС
    /// </summary>
    public double MeanHR { get; set; }
  }
}

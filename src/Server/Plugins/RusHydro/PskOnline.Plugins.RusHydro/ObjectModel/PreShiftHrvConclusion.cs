namespace PskOnline.Server.Plugins.RusHydro.ObjectModel
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [ComplexType]
  public class PreShiftHrvConclusion
  {
    public PreShiftHrvConclusion()
    {
      LSR = LSR_HrvFunctionalState.Critical_0;
      VSR = 0.001;
    }

    /// <summary>
    /// test id from the database where results are stored
    /// </summary>
    public Guid TestId { get; set; }

    /// <summary>
    /// status
    /// </summary>
    public PsaStatus Status { get; set; }

    /// <summary>
    /// text containing verbal representation of status
    /// </summary>
    [MaxLength(256)]
    public string Text { get; set; }

    // comment (may contain some detailed results)
    [MaxLength(256)]
    public string Comment { get; set; }

    public LSR_HrvFunctionalState LSR { get; set; }

    [MaxLength(256)]
    public string LSR_Text { get; set; }

    public double VSR { get; set; }

    public int StateMatrixRow { get; set; }

    public int StateMatrixCol { get; set; }

    /// <summary>
    /// Индекс напряжения (Баевский) / Tension Index (Bayevsky)
    /// </summary>
    public double IN { get; set; }

    /// <summary>
    /// Средняя ЧСС
    /// </summary>
    public double MeanHR { get; set; }
  }
}

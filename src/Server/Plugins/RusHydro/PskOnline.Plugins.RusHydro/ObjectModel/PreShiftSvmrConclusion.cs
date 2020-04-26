namespace PskOnline.Server.Plugins.RusHydro.ObjectModel
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [ComplexType]
  public class PreShiftSvmrConclusion
  {
    public PreShiftSvmrConclusion()
    {
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

    /// <summary>
    /// comment (may contain some detailed results)
    /// </summary>
    [MaxLength(256)]
    public string Comment { get; set; }

    /// <summary>
    /// ИПН1
    /// </summary>
    public double IPN1 { get; set; }

    /// <summary>
    /// Среднее время реакции, мс
    /// </summary>
    public double MeanResponseTimeMSec { get; set; }
  }
}

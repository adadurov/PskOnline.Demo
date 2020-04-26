namespace PskOnline.Methods.Hrv.ObjectModel
{
  using PskOnline.Methods.ObjectModel.Attributes;

  public class ResultsReliabilityEstimation
  {
    [Exportable]
    public int hc_marks_total = 0;

    [Exportable]
    public int hc_marks_2_int = 0;

    [Exportable]
    public int hc_marks_1_int = 0;

    [Exportable]
    public int hc_marks_0_int = 0;

    [Exportable]
    public bool reject_min_max_enabled = false;

    [Exportable]
    public double reject_nn_min = 0;

    [Exportable]
    public double reject_nn_max = 0;

    [Exportable]
    public bool reject_relative_enabled = false;

    [Exportable]
    public double reject_nn_relative = 0;
  }
}

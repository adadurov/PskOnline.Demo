namespace PskOnline.Methods.ObjectModel.Extensions
{
  public static class TestInfoExtensions
  {
    /// <summary>
    /// Массив возрастов всех обследуемых
    /// на момент окончания обследования в полных годах
    /// (только для экспорта).
    /// </summary>
//    [NeuroLab.BioMouse.common.export.Exportable]
    public static int GetPatientAgeAtTheTimeOfInspectionEnd(this TestInfo _this)
    {
       return _this.Patient.GetAgeAtGivenMoment(_this.InspectionTime);
    }

  }
}

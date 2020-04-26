namespace PskOnline.Methods.ObjectModel.Method
{
  using PskOnline.Methods.ObjectModel.PhysioData;

  /// <summary>
  /// Specific methods may add more fields to the basic structure
  /// </summary>
  public interface IMethodRawData
  {
    TestInfo TestInfo { get; set; }

    PatientPhysioData PhysioData { get; set; }
  }
}

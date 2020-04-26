namespace PskOnline.Methods.ObjectModel.Test
{
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.ObjectModel.PhysioData;
  using PskOnline.Methods.ObjectModel.Method;

  /// <summary>
  /// Summary description for TestRawData.
  /// </summary>
  public class TestRawData : IMethodRawData
  {
    public TestInfo TestInfo { get; set; }

    public PatientPhysioData PhysioData { get; set; }

	}
}

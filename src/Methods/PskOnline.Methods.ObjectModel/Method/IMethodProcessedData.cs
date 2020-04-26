namespace PskOnline.Methods.ObjectModel.Method
{
  using PskOnline.Methods.ObjectModel.Attributes;

  public interface IMethodProcessedData
  {
    [Exportable(100)]
    TestInfo TestInfo { get; set; }

  }
}

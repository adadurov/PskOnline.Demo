namespace PskOnline.Server.Plugins.RusHydro.Contracts
{
  using System;
  using System.Collections.Generic;
  using Newtonsoft.Json.Linq;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.Svmr.ObjectModel;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public interface ISummaryGenerator
  {
    SummaryDocument GenerateSummary(string databaseId, Guid inspectionId, Employee employee, IDictionary<TestInfo, JObject> testData);

    SummaryDocument GenerateSummary(string databaseId, Guid inspectionId, Employee employee, SvmrRawData svmrData, HrvRawData hrvData);
  }
}

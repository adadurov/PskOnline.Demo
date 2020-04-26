namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;
  using System.Linq;
  using System.Collections.Generic;

  using Newtonsoft.Json.Linq;

  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Svmr.ObjectModel;
  using PskOnline.Server.Plugins.RusHydro.Contracts;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  /// <summary>
  /// Формирует сводку по результатам предсменного контроля.
  /// </summary>
  public class PreShiftSummaryGenerator : ISummaryGenerator
  {
    private log4net.ILog _log = log4net.LogManager.GetLogger(typeof (PreShiftSummaryGenerator));

    public SummaryDocument GenerateSummary(string databaseId, Guid inspectionId, Employee employee, IDictionary<TestInfo, JObject> testData)
    {
      return GenerateSummaryInternal(databaseId, inspectionId, employee, testData);
    }

    private readonly string hrvMethodId = HrvMethodId.MethodId;

    private readonly string svmrMethodId = SvmrMethodId.MethodId;

    private static DateTimeOffset Max(DateTimeOffset d1, DateTimeOffset d2)
    {
      return d1 > d2 ? d1 : d2;
    }

    /// <summary>
    /// same as GenerateSummary, but returns the generated summary object
    /// </summary>
    /// <returns></returns>
    public SummaryDocument GenerateSummaryInternal(string databaseId, Guid inspectionId, Employee employee, IDictionary<TestInfo, JObject> testData)
    {
      _log.InfoFormat("Entered GenerateSummaryInternal() routine");

      if (testData.Count > 2) throw new InvalidOperationException("The results package must contain exactly 2 items");

      var hrvData = FindFirstMethodData<HrvRawData>(testData, HrvMethodId.MethodId);
      var svmrData = FindFirstMethodData<SvmrRawData>(testData, SvmrMethodId.MethodId);

      return GenerateSummary(databaseId, inspectionId, employee, svmrData, hrvData);
    }

    public SummaryDocument GenerateSummary(string databaseId, Guid inspectionId, Employee employee, SvmrRawData svmrData, HrvRawData hrvData)
    {
      TestIfDataAreAcceptable(hrvData, svmrData, out TestInfo hrvTestInfo, out TestInfo _);

      _log.Info($"Input data for employee '{employee}' seem to be accessible");

      DateTimeOffset completionDate = GetCompletionDate(hrvData, svmrData);

      var hostName = hrvTestInfo.MachineName;

      new RusHydroHrvAnalyzer().GetHrvConclusion(
        hrvData, hrvData.TestInfo.TestId, out PreShiftHrvConclusion hrvConclusion);
      new RusHydroSvmrAnalyzer().GetSvmrConclusion(
        svmrData, svmrData.TestInfo.TestId, out PreShiftSvmrConclusion svmrConclusion);

      var finalConclusion = FinalStatusProvider.GetFinalConclusion(hrvConclusion, svmrConclusion);
      finalConclusion.InspectionId = inspectionId;

      _log.Info($"Summary prepared for employee {employee}.");

      return new SummaryDocument
      {
        Employee = employee,
        CompletionTime = completionDate,
        HrvConclusion = hrvConclusion,
        SvmrConclusion = svmrConclusion,
        FinalConclusion = finalConclusion,
        HostName = hostName,
        InspectionId = inspectionId
      };
    }

    private DateTimeOffset GetCompletionDate(IMethodRawData hrvInputData, IMethodRawData svmrInputData)
    {
      var hrvCompletionTime = DateTimeOffset.MinValue;
      var svmrCompletionTime = DateTimeOffset.MinValue;

      if( null != hrvInputData.TestInfo)
      {
        hrvCompletionTime = hrvInputData.TestInfo.FinishTime;
      }

      if( null != svmrInputData.TestInfo)
      {
        svmrCompletionTime = svmrInputData.TestInfo.FinishTime;
      }

      return Max(hrvCompletionTime, svmrCompletionTime);
    }

    /// <summary>
    /// throws exception if something is wrong with the input data
    /// </summary>
    private void TestIfDataAreAcceptable(
      HrvRawData hrvData, 
      SvmrRawData svmrData, 
      out TestInfo hrvTestInfo, 
      out TestInfo svmrTestInfo)
    {
      if( null == hrvData )
      {
        throw new ArgumentOutOfRangeException(
          "This summary generator requires HRV & SVMR data. HRV test data not found."
          );
      }
      if (null == svmrData)
      {
        throw new ArgumentOutOfRangeException(
          "This summary generator requires HRV & SVMR data. SVMR test data not found."
          );
      }
      if( ReferenceEquals(hrvData, svmrData) )
      {
        throw new Exception("Logic error. Please report the error to solution vendor!");
      }

      svmrTestInfo = svmrData.TestInfo;
      hrvTestInfo = hrvData.TestInfo;
      if( ReferenceEquals(svmrTestInfo, hrvTestInfo) )
      {
        throw new Exception(
          "2 methods must have different result instances. Please report the error to developers!"
          );
      }

      if( svmrTestInfo.Patient == null )
      {
        throw new Exception("SVMR data doesn't have employee/patient information!");
      }

      if( hrvTestInfo.Patient == null )
      {
        throw new Exception("HRV data doesn't have employee/patient information!");
      }

      if(svmrTestInfo.Patient.Id != hrvTestInfo.Patient.Id )
      {
        throw new Exception(
          "Summary report will only be generated based on results belonging to a single employee!"
          );
      }
    }

    /// <summary>
    /// returns null if not found;
    /// each JObject is expected to contain an object of a type derifed from IMethodRawData
    /// and containing valid data in IMethodRawData.TestInfo property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="testData"></param>
    /// <param name="methodId"></param>
    /// <returns></returns>
    private T FindFirstMethodData<T>(IDictionary<TestInfo, JObject> testData, string methodId)
    {
      return testData
        .Where(c => c.Key.MethodId == methodId)
        .Select(c => c.Value.ToObject<T>())
        .FirstOrDefault();
    }

  }
}

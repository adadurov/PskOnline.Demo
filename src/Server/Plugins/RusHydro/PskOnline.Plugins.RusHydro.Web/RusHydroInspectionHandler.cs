namespace PskOnline.Server.Plugins.RusHydro.Web
{
  using Microsoft.AspNetCore.Http;
  using Microsoft.Extensions.Logging;
  using Newtonsoft.Json.Linq;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Svmr.ObjectModel;
  using PskOnline.Server.DAL.Inspections;
  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Plugins.RusHydro.DAL;
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;
  using PskOnline.Server.Plugins.RusHydro.Web.Logic;
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Plugins;
  using PskOnline.Server.Shared.Service;
  using System;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;

  public class RusHydroInspectionHandler : IInspectionResultsHandler
  {
    private readonly IInspectionService _inspectionService;
    private readonly ITestService _testService;
    private readonly IOrgStructureReference _orgStructRef;
    private readonly ILogger _logger;
    private readonly IDepartmentPsaReportService _departmentPsaReportService;
    private readonly IPsaSummaryService _summaryService;
    private readonly string _environmentId;

    public RusHydroInspectionHandler(IInspectionService inspections,
                                     ITestService tests,
                                     IOrgStructureReference orgStructRef,
                                     IPsaSummaryService summarySvc,
                                     IDepartmentPsaReportService deptPsaReportSvc,
                                     IHttpContextAccessor httpContextAccessor,
                                     ILogger<RusHydroInspectionHandler> logger
                                     )
    {
      _inspectionService = inspections;
      _testService = tests;
      _orgStructRef = orgStructRef;
      _summaryService = summarySvc;
      _departmentPsaReportService = deptPsaReportSvc;
      _logger = logger;

      var hostString = httpContextAccessor.HttpContext.Request.Host;
      _environmentId = hostString.Value ?? "psk-online/unknown";
    }

    public string GetMethodSetId()
    {
      return RushydroPsaMethodSetId.Value;
    }

    public string GetPluginSlug()
    {
      return "rushydro-psa";
    }

    public async Task<string> ProcessInspectionResultsAsync(Guid inspectionId, CancellationToken cancellationToken)
    {
      var generator = new PreShiftSummaryGenerator();
      var testData = await GetTestDataAsync(inspectionId);
      if (testData == null)
      {
        return null;
      }

      var summary = generator.GenerateSummary(_environmentId, inspectionId, testData.Employee, testData.SvmrRawData, testData.HrvRawData);
      summary.DepartmentId = testData.Inspection.DepartmentId;
      summary.BranchOfficeId = testData.Inspection.BranchOfficeId;

      // add shift start date & number
      summary.WorkingShiftDate = RusHydroScheduler.GetShiftStartDate(
        summary.CompletionTime.LocalDateTime, out bool isDayShift, out int shiftNumber);
      summary.WorkingShiftNumber = shiftNumber;

      await StoreSummary(summary);

      return $"api/plugins/rushydro-psa/summary/inspection/{inspectionId}";
    }

    private async Task StoreSummary(SummaryDocument summary)
    {
      try
      {
        await _summaryService.AddAsync(summary);
      }
      catch( ItemAlreadyExistsException ex )
      {
        _logger.LogInformation($"Summary for inspection '{summary.InspectionId}' already stored in the database with ID='{ex.Id}'.");
      }
      await _departmentPsaReportService.AddSummaryAsync(summary);
    }

    private async Task<TestData> GetTestDataAsync(Guid inspectionId)
    {
      var inspection = await _inspectionService.GetInspectionWithTestsAsync(inspectionId);

      var hrvTest = inspection.Tests
        .Where(t => t.MethodId == HrvMethodId.MethodId).FirstOrDefault();
      var svmrTest = inspection.Tests
        .Where(t => t.MethodId == SvmrMethodId.MethodId).FirstOrDefault();
      if( hrvTest == null )
      {
        _logger.LogWarning($"Could not find HRV test data in the inspection '{inspectionId}'");
        return null;
      }
      if( svmrTest == null )
      {
        _logger.LogWarning($"Could not find SVMR test data in the inspection '{inspectionId}'");
        return null;
      }
      var hrvRawData = JObject.Parse(hrvTest.MethodRawDataJson).ToObject<HrvRawData>();
      var svmrRawData = JObject.Parse(svmrTest.MethodRawDataJson).ToObject<SvmrRawData>();

      var pskEmp = await _orgStructRef.GetEmployeeAsync(inspection.EmployeeId);
      var branch = await _orgStructRef.GetBranchOfficeAsync(pskEmp.BranchOfficeId);
      var dept = await _orgStructRef.GetDepartmentAsync(pskEmp.DepartmentId);
      var pos = await _orgStructRef.GetPositionAsync(pskEmp.PositionId);

      var rhEmp = new RusHydro.ObjectModel.Employee
      {
        Id = pskEmp.Id,
        FullName = pskEmp.FullName,
        ExternalId = pskEmp.ExternalId,
        BranchOfficeName = branch?.Name,
        BranchOfficeId = pskEmp.BranchOfficeId,
        DepartmentName = dept?.Name,
        DepartmentId = pskEmp.DepartmentId,
        PositionName = pos?.Name,
        PositionId = pskEmp.PositionId,
      };

      hrvRawData.TestInfo.TestId = hrvTest.Id;
      svmrRawData.TestInfo.TestId = svmrTest.Id;

      return new TestData
      {
        Employee = rhEmp,
        Inspection = inspection,
        HrvRawData = hrvRawData,
        SvmrRawData = svmrRawData
      };
    }
  }
}

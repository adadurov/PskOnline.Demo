namespace PskOnline.Service.Test.Integration.UAT
{
  using System;
  using System.Linq;
  using System.Net.Http;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Mvc.Testing;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Linq;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Inspection;
  using PskOnline.Client.Api.OpenId;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Components.Log;
  using PskOnline.Components.Util;
  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.ObjectModel.Test;
  using PskOnline.Methods.Svmr.ObjectModel;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Plugins.RusHydro;
  using PskOnline.Server.Plugins.RusHydro.Web.Dto;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description = "Performs tests related to RusHydro inspection results handling plugin." +
                  "Notice that this test fixture is initialized only once and tests should run in order!")]
  public class RusHydro_Plugin_Test : IDisposable
  {
    DefaultWebApplicationFactory _app;

    HttpClient _httpClient;

    IApiClient _apiClient;

    TenantContainer _gryffindorHouse;

    TenantContainer _slytherinHouse;

    string _workDepartmentId;

    WorkplaceCredentialsDto _operatorWorkplaceCredentials;
    WorkplaceCredentialsDto _auditorWorkplaceCredentials;
    const string RusHydroPluginTypeId = "rushydro-psa";

    [SetUp]
    public async Task SetUp()
    {
      await InitOnce();

      // start each test as Gryffindor user
      await _apiClient.AsGryffindorAdminAsync(_httpClient);

      if (_operatorWorkplaceCredentials == null)
      {
        await CreateDepartmentCredentialsAsync();
      }
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
      _httpClient = null;
      _app?.Dispose();
      _app = null;
      LogHelper.ShutdownLogSystem();
    }

    private async Task InitOnce()
    {
      if (_app != null) return;

      LogHelper.ConfigureConsoleLogger();

      _app = new DefaultWebApplicationFactory();

      var options = new WebApplicationFactoryClientOptions {AllowAutoRedirect = false };
      _httpClient = _app.CreateClient(options);

      var apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());
      apiClient.TraceResponseOnException = true;

      _apiClient = apiClient;

      // site admin creates 2 tenants
      await _apiClient.AsSiteAdminAsync(_httpClient);
      _gryffindorHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);

      await _apiClient.AsSiteAdminAsync(_httpClient);
      _slytherinHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.SlytherinHouse);

      // Slytherin House admin creates branch office and departments
      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _slytherinHouse).Wait();
      TestDepartment.SeedDefaultDepartments(_apiClient, _slytherinHouse).Wait();
      TestPosition.SeedDefaultPosition(_apiClient, _slytherinHouse).Wait();
      TestEmployees.SeedDefaultEmployees(_apiClient, _slytherinHouse).Wait();

      // Gryffindor House admin creates branch office and departments
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _gryffindorHouse).Wait();
      TestDepartment.SeedDefaultDepartments(_apiClient, _gryffindorHouse).Wait();
      TestPosition.SeedDefaultPosition(_apiClient, _gryffindorHouse).Wait();
      TestEmployees.SeedDefaultEmployees(_apiClient, _gryffindorHouse).Wait();

      _workDepartmentId = _gryffindorHouse.Department_1_2.Department.Id;
    }

    private async Task CreateDepartmentCredentialsAsync()
    {
      var uri = $"/api/department/{_workDepartmentId}/workplace";
      {
        var workplaceDescriptor = new WorkplaceCredentialsRequestDto
        {
          Scopes = PskOnlineScopes.DeptOperatorWorkplace
        };
        var response = await _httpClient.PostAsJsonAsync(uri, workplaceDescriptor);

        // Then
        // the request should succeed
        response.EnsureSuccessStatusCode();
        _operatorWorkplaceCredentials = await response.Content.ReadAsJsonAsync<WorkplaceCredentialsDto>();
      }
      {
        var workplaceDescriptor = new WorkplaceCredentialsRequestDto
        {
          Scopes = PskOnlineScopes.DeptAuditorWorkplace
        };

        // Then
        // the request should succeed
        var response = await _httpClient.PostAsJsonAsync(uri, workplaceDescriptor);
        response.EnsureSuccessStatusCode();

        _auditorWorkplaceCredentials = await response.Content.ReadAsJsonAsync<WorkplaceCredentialsDto>();
      }
    }

    [Test]
    [Order(20)]
    public async Task CompleteInspection_ShouldReturnReferenceToRusHydroSummary_2()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      var employees = await _apiClient.GetDepartmentEmployeesAsync(_workDepartmentId);

      // When
      // the client submits a new inspection with a special Rushydro method set ID
      var inspectionData = GetInspectionData(
        employees.First(), 
        GetCurrentIrkutskTimeWithOffset(TimeSpan.FromMinutes(-10)),
        SampleTestDataJson.Sample_995_Hrv_Pass,
        SampleTestDataJson.Sample_995_Svmr_
        );
      var completionResponse = await UploadInspectionData(inspectionData);

      var rushydroResult = completionResponse
        .PluginResults.FirstOrDefault(p => p.PluginType == RusHydroPluginTypeId);

      // Then
      // the server should return a reference to a new summary generated by RusHydro plugin
      Assert.NotNull(rushydroResult);
      Assert.IsNotEmpty(rushydroResult.ResultsUrl);
    }

    [Test]
    [Order(25)]
    public async Task CompleteInspection_ShouldSucceed_GivenIntervalData()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      var employees = await _apiClient.GetDepartmentEmployeesAsync(_workDepartmentId);

      // When
      // the client submits a new inspection with a special Rushydro method set ID
      var inspectionData = GetInspectionData(
        employees.First(),
        GetCurrentIrkutskTimeWithOffset(TimeSpan.FromMinutes(-10)),
        SampleTestDataJson.Sample_Hrv_Intervals,
        SampleTestDataJson.Sample_995_Svmr_
        );
      var completionResponse = await UploadInspectionData(inspectionData);

      var rushydroResult = completionResponse
        .PluginResults.FirstOrDefault(p => p.PluginType == RusHydroPluginTypeId);

      // Then
      // the server should return a reference to a new summary generated by RusHydro plugin
      Assert.NotNull(rushydroResult);
      Assert.IsNotEmpty(rushydroResult.ResultsUrl);
    }

    private async Task<InspectionCompleteResponseDto> UploadInspectionData(RusHydroInspectionData data)
    {
      var inspectionId = await _apiClient.PostInspectionAsync(data.Inspection);
      data.SvmrTest.InspectionId = data.HrvTest.InspectionId = inspectionId.ToString();
      var hrvTestId = await _apiClient.PostTestAsync(data.SvmrTest);
      var svmrTestId = await _apiClient.PostTestAsync(data.HrvTest);

      var completeDto = new InspectionCompleteDto
      {
        Id = inspectionId,
        FinishTime = data.SvmrTest.FinishTime > data.HrvTest.FinishTime ?
                     data.SvmrTest.FinishTime : data.HrvTest.FinishTime
      };

      return await _apiClient.CompleteInspectionAsync(completeDto);
    }

    public class RusHydroInspectionData
    {
      public InspectionPostDto Inspection { get; set; }

      public TestPostDto HrvTest { get; set; }

      public TestPostDto SvmrTest { get; set; }
    }

    private RusHydroInspectionData GetInspectionData(
      EmployeeDto employee,
      DateTimeOffset desiredFinishTime,
      string hrvDataJson,
      string svmrDataJson
      )
    {
      Tuple<T, JObject> ReadPskJsonFormat<T>(string content)
      {
        // parse json & check signature
        var json = JObject.Parse(content);
        TestDataJsonFormat_0_1.CheckSignature(json);
        var testDataJson = TestDataJsonFormat_0_1.GetTestData(json);
        // extract the method-specific json
        return Tuple.Create(TestDataJsonFormat_0_1.GetTestData<T>(json), testDataJson);
      }

      TestPostDto MakeTestDto(TestInfo testInfo, JObject rawData)
      {
        // replace patient info with a real employee data
        testInfo.Patient.Id = employee.Id;
        testInfo.Patient.Name = string.Join(employee.LastName, employee.FirstName, employee.Patronymic);
        testInfo.Patient.BirthDate = employee.BirthDate;
        testInfo.Patient.Gender = (Gender)employee.Gender;
        testInfo.Patient.BranchOfficeId = employee.BranchOfficeId;
        testInfo.Patient.BranchOfficeName = "";
        testInfo.Patient.DepartmentId = employee.DepartmentId;
        testInfo.Patient.DepartmentName = "";
        testInfo.Patient.PositionId = employee.PositionId;
        testInfo.Patient.PositionName = "";

        return new TestPostDto
        {
          MethodId = testInfo.MethodId,
          Comment = testInfo.Comment,
          FinishTime = testInfo.FinishTime,
          StartTime = testInfo.StartTime,
          MethodVersion = testInfo.MethodModuleVersion,
          MethodRawDataJson = rawData
        };
      }

      var hrvData = ReadPskJsonFormat<HrvRawData>(hrvDataJson);
      var svmrData = ReadPskJsonFormat<SvmrRawData>(svmrDataJson);
      // adjust test start time
      hrvData = UpdateTestTime(hrvData, desiredFinishTime, TimeSpan.FromMinutes(3));
      svmrData = UpdateTestTime(svmrData, desiredFinishTime - TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(2));
      var svmrTestInfo = svmrData.Item1.TestInfo;
      var hrvTestInfo = hrvData.Item1.TestInfo;
      return new RusHydroInspectionData
      {
        SvmrTest = MakeTestDto(svmrTestInfo, svmrData.Item2),
        HrvTest = MakeTestDto(hrvTestInfo, hrvData.Item2),
        Inspection = new InspectionPostDto
        {
          EmployeeId = employee.Id,
          StartTime = svmrTestInfo.StartTime < hrvTestInfo.StartTime 
                        ? svmrTestInfo.StartTime : hrvTestInfo.StartTime,
          InspectionPlace = PskOnline.Client.Api.Inspection.InspectionPlace.OnWorkplace,
          InspectionType = PskOnline.Client.Api.Inspection.InspectionType.PreShift,
          MachineName = "made-up machine name",
          MethodSetId = RushydroPsaMethodSetId.Value,
        }
      };
    }

    private Tuple<T, JObject> UpdateTestTime<T>(
      Tuple<T, JObject> testData, 
      DateTimeOffset desiredFinishTime, TimeSpan duration) where T : TestRawData
    {
      testData.Item1.TestInfo.StartTime = desiredFinishTime - duration;
      testData.Item1.TestInfo.FinishTime = desiredFinishTime;

      return Tuple.Create(testData.Item1, JObject.FromObject(testData.Item1));
    }

    private DateTimeOffset GetCurrentIrkutskTimeWithOffset(TimeSpan timeSpan)
    {
      var serverTime = DateTime.UtcNow;
      var prepLocalTime = new DateTime(
        serverTime.Year, serverTime.Month, serverTime.Day,
        serverTime.Hour, serverTime.Minute, serverTime.Second, DateTimeKind.Unspecified);

      var irtkutsTimeZoneUtcOffset = TimeSpan.FromHours(5 + 3);

      var irkutskLocalTime = new DateTimeOffset(prepLocalTime + irtkutsTimeZoneUtcOffset, irtkutsTimeZoneUtcOffset);

      return irkutskLocalTime;
    }


    [Test]
    [Order(30)]
    public async Task Get_Summary_ShouldSucceed_GivenOperatorWorkplace()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      var employees = await _apiClient.GetDepartmentEmployeesAsync(_workDepartmentId);
      var branchOfficeId = _apiClient.GetIdToken().GetBranchOfficeIdClaimValue();
      var departmentId = _apiClient.GetIdToken().GetDepartmentIdClaimValue();

      var inspectionData = GetInspectionData(
        employees.First(), 
        GetCurrentIrkutskTimeWithOffset(TimeSpan.FromMinutes(+10)),
        SampleTestDataJson.Sample_995_Hrv_Pass,
        SampleTestDataJson.Sample_995_Svmr_
        );
      var completionResponse = await UploadInspectionData(inspectionData);

      var rushydroResult = completionResponse
        .PluginResults.FirstOrDefault(p => p.PluginType == RusHydroPluginTypeId);

      var summary = default(PsaSummaryDto);

      try
      {
        // When
        // the client submits a new inspection with a special Rushydro method set ID
        // When
        // the operator requests the RusHydro summary for the completed inspection
        summary = await _apiClient.HttpGetAsJsonFromRelativeUriAsync<PsaSummaryDto>(
          rushydroResult.ResultsUrl);
      }
      catch( Exception ex )
      {
        Console.WriteLine(ex.Message);
        throw;
      }

      // Then
      // the request should succeed (should not throw exceptions)
//      Assert.That(report.DepartmentId, Is.EqualTo(_workDepartmentId));

      Assert.That(summary.BranchOfficeId, Is.EqualTo(branchOfficeId));
      Assert.That(summary.CompletionTime, Is.GreaterThan(inspectionData.Inspection.StartTime));

      Assert.That(Guid.Parse(summary.HrvConclusion.TestId), Is.Not.EqualTo(Guid.Empty));
      Assert.That(summary.HrvConclusion.IN, Is.Not.EqualTo(0.0d));
      Assert.That(summary.HrvConclusion.MeanHR, Is.Not.EqualTo(0.0d));
      Assert.That(summary.HrvConclusion.StateMatrixRow != 0);
      Assert.That(summary.HrvConclusion.StateMatrixCol != 0);

      Assert.That(Guid.Parse(summary.SvmrConclusion.TestId), Is.Not.EqualTo(Guid.Empty));
      Assert.That(summary.SvmrConclusion.IPN1, Is.Not.EqualTo(0.0d));
      Assert.That(summary.SvmrConclusion.MeanResponseTimeMSec, Is.Not.EqualTo(0.0d));

      Assert.That(summary.WorkingShiftDate.Date, Is.LessThanOrEqualTo(summary.CompletionTime.LocalDateTime));
      Assert.That(summary.WorkingShiftNumber, Is.GreaterThan(0));

    }

    [Test]
    [Order(31)]
    public async Task Get_Summary_Should_Return_404_Given_Bad_Inspection_Id()
    {
      // Given
      // client authenticated as operator workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      var employees = await _apiClient.GetDepartmentEmployeesAsync(_workDepartmentId);

      var inspectionData = GetInspectionData(
        employees.First(),
        GetCurrentIrkutskTimeWithOffset(TimeSpan.FromMinutes(5)),
        SampleTestDataJson.Sample_995_Hrv_Pass,
        SampleTestDataJson.Sample_995_Svmr_
        );
      var completionResponse = await UploadInspectionData(inspectionData);

      // having submitted a new inspection with a special Rushydro method set ID
      var rushydroResult = completionResponse
        .PluginResults.FirstOrDefault(p => p.PluginType == RusHydroPluginTypeId);

      var badUrl = rushydroResult.ResultsUrl;
      badUrl = badUrl.Substring(0, badUrl.LastIndexOf('/')) + Guid.NewGuid().ToString();

      // When
      // the operator requests the RusHydro summary for the completed inspection but with a bad ID
      AsyncTestDelegate action = async () => 
        await _apiClient.HttpGetAsJsonFromRelativeUriAsync<PsaSummaryDto>(badUrl);

      // Then
      // the request should return 404 - not found
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    [Order(32)]
    public async Task Get_Summary_Should_Return_403_Given_Wrong_Tenant()
    {
      // Given
      // client authenticated as operator workplace in Gryffindor
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _operatorWorkplaceCredentials.ClientId,
        _operatorWorkplaceCredentials.ClientSecret,
        PskOnlineScopes.DeptOperatorWorkplace);

      var employees = await _apiClient.GetDepartmentEmployeesAsync(_workDepartmentId);
      var branchOfficeId = _apiClient.GetIdToken().GetBranchOfficeIdClaimValue();
      var departmentId = _apiClient.GetIdToken().GetDepartmentIdClaimValue();

      var inspectionData = GetInspectionData(
        employees.First(),
        GetCurrentIrkutskTimeWithOffset(TimeSpan.FromMinutes(5)),
        SampleTestDataJson.Sample_995_Hrv_Pass,
        SampleTestDataJson.Sample_995_Svmr_
        );
      var completionResponse = await UploadInspectionData(inspectionData);

      // having submitted a new inspection with a special Rushydro method set ID
      var rushydroResult = completionResponse
        .PluginResults.FirstOrDefault(p => p.PluginType == RusHydroPluginTypeId);

      // When
      // a user from a different tenant requests the summary for the completed inspection
      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      AsyncTestDelegate action = async () =>
        await _apiClient.HttpGetAsJsonFromRelativeUriAsync<PsaSummaryDto>(rushydroResult.ResultsUrl);

      // Then
      // the request should return 403 - Unauthorized
      Assert.ThrowsAsync<ItemNotFoundException>(action);

      // And the same URL requested by Gryffindor admin should succeed
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      var summary = await _apiClient.HttpGetAsJsonFromRelativeUriAsync<PsaSummaryDto>(rushydroResult.ResultsUrl);

      Assert.That(summary.BranchOfficeId, Is.EqualTo(branchOfficeId));
    }

    [Test]
    [Order(40)]
//    [Ignore("This functionality is not yet implemented")]
    public async Task Get_PsaReport_ShouldSucceed_GivenAuditorWorkplace()
    {
      // Given
      // client authenticated as auditor workplace
      await _apiClient.SignInWithWorkplaceCredentialsAsync(
        _auditorWorkplaceCredentials.ClientId,
        _auditorWorkplaceCredentials.ClientSecret, 
        PskOnlineScopes.DeptAuditorWorkplace);

      var workplaceIdToken = _apiClient.GetIdToken();
      var departmentId = workplaceIdToken.Claims.First(c => c.Type == CustomClaimTypes.DepartmentId).Value;

      // When
      // the auditor requests the department PSA report
      var report = await _apiClient.HttpGetAsJsonFromRelativeUriAsync<PsaReportDto>(
        $"api/plugins/rushydro-psa/PsaReport/department/{departmentId}/current");

      Console.WriteLine(JsonConvert.SerializeObject(report));

      // Then
      // the request should succeed (should not throw exceptions)
      Assert.That(report.Summaries.Length, Is.GreaterThan(0));
      Assert.That(report.Summaries.Length, Is.LessThanOrEqualTo(4));

      foreach( var summary in report.Summaries )
      {
        Assert.That(Guid.Parse(summary.HrvConclusion.TestId), Is.Not.EqualTo(Guid.Empty));
        Assert.That(Guid.Parse(summary.SvmrConclusion.TestId), Is.Not.EqualTo(Guid.Empty));
      }

    }
  }
}

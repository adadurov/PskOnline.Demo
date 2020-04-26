namespace PskOnline.Service.Test.Controllers
{
  using System;
  using System.Net;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Mvc;

  using NUnit.Framework;
  using NSubstitute;
  using Shouldly;

  using AutoMapper;
  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service;
  using PskOnline.Server.Service.Controllers;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Exceptions;
  using System.Threading;

  [TestFixture]
  public class InspectionController_Tests
  {
    IInspectionService _inspectionService;
    ITenantIdProvider _tenantIdProvider;
    Guid _newId;
    Guid _tenantId;
    Inspection _incompleteInspection;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      StaticStartup.Startup();

      _newId = Guid.NewGuid();
      _tenantId = Guid.NewGuid();
      _inspectionService = Substitute.For<IInspectionService>();
      _inspectionService.When(s => s.BeginInspectionAsync(Arg.Any<Inspection>()))
        .Do(v => ((Inspection)v[0]).Id = _newId);
      _inspectionService.BeginInspectionAsync(Arg.Any<Inspection>()).ReturnsForAnyArgs(_newId);
      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(_tenantId);

      _incompleteInspection = new Inspection()
      {

      };
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void InspectionDto_To_Inspection()
    {
      var newInspectionModel = new InspectionDto
      {
        MethodSetId = "Method_Set_Id",
        StartTime = DateTime.Now - TimeSpan.FromMinutes(7),
        FinishTime = DateTime.Now,
        InspectionPlace = InspectionPlace.OnWorkplace,
        InspectionType = InspectionType.PreShift,
        EmployeeId = Guid.NewGuid().ToString(),
        BranchOfficeId = Guid.NewGuid().ToString(),
        DepartmentId = Guid.NewGuid().ToString(),
        MachineName = "HOST002",
        MethodSetVersion = "Version 16"
      };

      var inspection = Mapper.Map<Inspection>(newInspectionModel);

      var inspectionDto = Mapper.Map<InspectionDto>(inspection);
    }

    [Test]
    public async Task GetAll_Should_Call_Get_All()
    {
      // Given
      var controller = new InspectionController(_inspectionService, _tenantIdProvider);

      var allResponse = controller.GetAll(null, null);

      await _inspectionService.Received().GetAllAsync(null, null);
    }

    [Test]
    public void Get_ShouldFail_GivenInspectionDoesntExist()
    {
      // Given
      var inspectionService = Substitute.For<IInspectionService>();
      inspectionService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "inspection"));
      var controller = new InspectionController(inspectionService, _tenantIdProvider);

      // When
      AsyncTestDelegate action = async () => await controller.GetInspection(Guid.NewGuid());

      // Then
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public void BeginInspection_Should_Call_BeginInspectionAsync_And_Return_Created_()
    {
      // Given
      var controller = new InspectionController(_inspectionService, _tenantIdProvider);

      var newInspectionModel = new InspectionDto
      {

        MethodSetId = "Method_Set_Id",
        StartTime = DateTime.Now - TimeSpan.FromMinutes(7),
        FinishTime = DateTime.Now,
        InspectionPlace = InspectionPlace.OnWorkplace,
        InspectionType = InspectionType.PreShift,
        EmployeeId = Guid.NewGuid().ToString(),
        BranchOfficeId = Guid.NewGuid().ToString(),
        DepartmentId = Guid.NewGuid().ToString(),
        MachineName = "HOST002",
        MethodSetVersion = "Version 16"
      };

      // When
      var response = controller.PostInspection(newInspectionModel);

      // Then
      _inspectionService.Received().BeginInspectionAsync(Arg.Any<Inspection>());
      _inspectionService.Received().BeginInspectionAsync(
        Arg.Is<Inspection>(v => v.StartTime == newInspectionModel.StartTime && 
        v.TenantId == _tenantId));

      var createdResponse = (CreatedResult)response.Result;
      Assert.That(createdResponse.Location, Is.EqualTo(nameof(InspectionController.GetInspection)));

      var createdAttribs = (CreatedWithGuidDto)createdResponse.Value;
      Assert.That(createdAttribs.Id, Is.EqualTo(_newId));
    }

    [Test]
    public async Task CompleteInspection_Should_Call_CompleteInspectionAsync_And_Return_NoContent()
    {
      // Given
      var controller = new InspectionController(_inspectionService, _tenantIdProvider);

      var completionDto = new InspectionCompleteDto
      {
        Id = _incompleteInspection.Id,
        FinishTime = DateTimeOffset.Now
      };

      // When
      // user tries to mark inspection as completed
      var response = (OkObjectResult)await controller.PutInspectionComplete(completionDto.Id, completionDto, default(CancellationToken));

      // Then
      // The controller calls service with proper arguments
      await _inspectionService.Received().CompleteInspectionAsync(
        Arg.Is<Guid>(g => g == completionDto.Id),
        Arg.Is<DateTimeOffset>( t => t == completionDto.FinishTime),
        Arg.Any<CancellationToken>());

      response.StatusCode.ShouldBe((int)HttpStatusCode.OK);
    }
  }
}
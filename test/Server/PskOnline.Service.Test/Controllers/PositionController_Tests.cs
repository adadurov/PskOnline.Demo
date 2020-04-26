namespace PskOnline.Service.Test.Controllers
{
  using Microsoft.Extensions.Logging;
  using NSubstitute;
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service;
  using PskOnline.Server.Service.Controllers;
  using PskOnline.Server.Service.ViewModels;
  using PskOnline.Server.Shared.Contracts;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Multitenancy;
  using System;
  using System.Collections.Generic;
  using System.Text;
  using PskOnline.Server.Shared.Exceptions;

  [TestFixture]
  public class PositionController_Tests
  {
    IService<Position> _positionService;
    ITenantIdProvider _tenantIdProvider;
    Guid _newId;
    Guid _tenantId;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      StaticStartup.Startup();

      _newId = Guid.NewGuid();
      _tenantId = Guid.NewGuid();
      _positionService = Substitute.For<IService<Position>>();
      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(_tenantId);

    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void Update_ShouldFail_GivenPositionDoesntExist()
    {
      // Given
      // PositionController over a mock service that would alway throw exceptions on Get & GetAsync
      _positionService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "position"));

      var positionController = new PositionController(
        _positionService, _tenantIdProvider, Substitute.For<ILogger<PositionController>>());

      var positionId = Guid.NewGuid();
      var positionDto = new PositionDto
      {
        Id = positionId.ToString(),
        Name = "Position"
      };

      // When
      // Put is called on the controller

      AsyncTestDelegate action = async () => await positionController.Put(positionId, positionDto);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }

    [Test]
    public void Get_ShouldFail_GivenPositionDoesntExist()
    {
      // Given
      // PositionController over a mock service that would alway throw exceptions on Get & GetAsync
      _positionService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "position"));

      var positionController = new PositionController(
        _positionService, _tenantIdProvider, Substitute.For<ILogger<PositionController>>());

      var positionId = Guid.NewGuid();

      // When
      // Put is called on the controller

      AsyncTestDelegate action = async () => await positionController.GetPosition(positionId);

      // Then
      // The controller should thrown ItemNotFoundException
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }
  }
}

namespace PskOnline.Service.Test.Controllers
{
  using System;

  using NUnit.Framework;
  using NSubstitute;

  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service;
  using PskOnline.Server.Service.Controllers;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.DAL.Inspections;
  using PskOnline.Server.Service.ViewModels;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Newtonsoft.Json.Linq;

  [TestFixture]
  public class TestController_Tests
  {
    ITestService _testService;
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
      _testService = Substitute.For<ITestService>();
      _tenantIdProvider = Substitute.For<ITenantIdProvider>();
      _tenantIdProvider.GetTenantId().Returns(_tenantId);
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void Get_ShouldFail_GivenTestDoesntExist()
    {
      // Given
      var testService = Substitute.For<ITestService>();
      testService.When(s => s.GetAsync(Arg.Any<Guid>()))
        .Do(v => throw new ItemNotFoundException(v[0].ToString(), "test"));
      var controller = new TestController(testService, _tenantIdProvider);

      // When
      AsyncTestDelegate action = async () => await controller.GetTest(Guid.NewGuid());

      // Then
      Assert.ThrowsAsync<ItemNotFoundException>(action);
    }
  }
}
namespace PskOnline.Service.Test.Services
{
  using System;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  using NUnit.Framework;
  using NSubstitute;
  using Moq;
  using PskOnline.Components.Log;

  using PskOnline.Server.DAL.Inspections;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Repository;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Exceptions;

  [TestFixture]
  class TestServiceTest
  {
    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    private ITestService GetTestServiceInstance(
      IGuidKeyedRepository<Test> repository,
      IGuidKeyedRepository<Inspection> inspectionRepo)
    {
      return new TestService(
        Substitute.For<ITenantEntityAccessChecker>(),
        Substitute.For<IAccessScopeFilter>(),
        repository,
        inspectionRepo,
        Substitute.For<ILoggerFactory>());
    }

    [Test]
    public async Task ShouldAddAndAssignProperIds_GivenNoDuplicatesExists()
    {
      // Given // Arrange
      var johnDoe = GetJohnDoeEmployee();
      var inspection = GetStartedInspectionInstance(johnDoe);
      var test = GetTest(inspection);

      // empty test repositoryf
      var mockTestRepo = Substitute.For<IGuidKeyedRepository<Test>>();
      mockTestRepo.SetupRepositoryWithEntity(null);

      // repository containing the started inspection...
      var mockInspectionRepo = Substitute.For<IGuidKeyedRepository<Inspection>>();
      mockInspectionRepo.SetupRepositoryWithEntity(inspection);

      // repository containing the employee that matches the test & inspection
      var mockEmployeeRepo = Substitute.For<IGuidKeyedRepository<Employee>>();
      mockEmployeeRepo.SetupRepositoryWithEntity(johnDoe);

      var testService = GetTestServiceInstance(mockTestRepo, mockInspectionRepo);

      // When // Act
      await testService.AddAsync(test);

      // Then // Assert
      mockTestRepo.Received().Add(Arg.Is<Test>(e => e.Id != Guid.Empty));
      await mockTestRepo.Received().SaveChangesAsync();

      Assert.That(test.EmployeeId, Is.EqualTo(inspection.EmployeeId));
      Assert.That(test.DepartmentId, Is.EqualTo(inspection.DepartmentId));
      Assert.That(test.BranchOfficeId, Is.EqualTo(inspection.BranchOfficeId));
    }

    [Test]
    public void ShouldNotAcceptDuplicateTest()
    {
      // Given // Arrange
      var johnDoe = GetJohnDoeEmployee();
      var inspection = GetStartedInspectionInstance(johnDoe);
      var test = GetTest(inspection);

      var mockRepo = new Mock<IGuidKeyedRepository<Test>>();
      mockRepo.Setup(m => m.GetSingleOrDefault(It.IsAny<Expression<Func<Test, bool>>>())).Returns(test);
      mockRepo.Setup(m => m.GetSingleOrDefaultAsync(It.IsAny<Expression<Func<Test, bool>>>())).Returns(Task.FromResult(test));
      var mockInspectionRepo = Substitute.For<IGuidKeyedRepository<Inspection>>();
      // repository containint the employee
      var mockEmployeeRepo = Substitute.For<IGuidKeyedRepository<Employee>>();
      mockEmployeeRepo.GetAsync(Arg.Any<Guid>())
        .Returns(new Employee { Id = test.EmployeeId, DepartmentId = test.DepartmentId, BranchOfficeId = test.BranchOfficeId });

      var service = GetTestServiceInstance(mockRepo.Object, mockInspectionRepo);

      // When // Act
      Task AsyncTestDelegate() => service.AddAsync(test);

      // Then // Assert
      Assert.ThrowsAsync<ItemAlreadyExistsException>(AsyncTestDelegate);
    }

    private static Employee GetJohnDoeEmployee()
    {
      return new Employee
      {
        Id = Guid.NewGuid(),
        TenantId = Guid.NewGuid(),
        DepartmentId = Guid.NewGuid(),
        BranchOfficeId = Guid.NewGuid()
      };
    }

    private static Inspection GetStartedInspectionInstance(Employee employee)
    {
      return new Inspection()
      {
        Id = Guid.NewGuid(),
        StartTime = DateTime.Now - TimeSpan.FromMinutes(6),
        FinishTime = null,
        MethodSetId = "123",
        TenantId = employee.TenantId,
        EmployeeId = employee.Id
      };
    }

    private static Test GetTest(Inspection inspection)
    {
      return new Test()
      {
        Id = Guid.NewGuid(),
        StartTime = DateTime.Now - TimeSpan.FromMinutes(6),
        FinishTime = DateTime.Now,
        MethodId = "123",
        InspectionId = inspection.Id,
        TenantId = inspection.TenantId
      };
    }
  }
}

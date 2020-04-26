namespace PskOnline.Service.Test.Services
{
  using System;
  using System.Linq.Expressions;
  using System.Collections.Generic;
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
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.ObjectModel;
  using System.Threading;

  [TestFixture]
  class InspectionServiceTest
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

    private IInspectionService GetServiceInstance(
      IGuidKeyedRepository<Inspection> repository,
      IGuidKeyedRepository<Test> testRepository,
      IGuidKeyedRepository<Employee> employeeRepository)
    {
      return new InspectionService(
        Substitute.For<ITenantEntityAccessChecker>(),
        Substitute.For<IAccessScopeFilter>(),
        repository,
        testRepository,
        employeeRepository,
        Substitute.For<IInspectionCompletionEventHandler>(),
        Substitute.For<ILoggerFactory>());
    }

    [Test]
    public async Task ShouldCompleteInspectionWithOneTest()
    {
      // Given an inspection
      var johnDoe = GetJohnDoeEmployee();
      var inspectionWithTest = GetStartedInspectionInstance(johnDoe);

      var mockInspectionRepo = GetMockInspectionRepo(inspectionWithTest);
      var mockTestRepo = MockMatchingTestRepo(inspectionWithTest);
      var mockEmployeeRepo = MockMatchingEmployeeRepo(inspectionWithTest);

      var service = GetServiceInstance(mockInspectionRepo, mockTestRepo, mockEmployeeRepo);

      // When
      await service.CompleteInspectionAsync(inspectionWithTest.Id, DateTime.Now, default(CancellationToken));

      // Then
      Assert.That(inspectionWithTest.IsFinished, Is.True);
    }

    [Test]
    public void ShouldNotCompleteEmptyInspection()
    {
      // Given
      var johnDoe = GetJohnDoeEmployee();
      var inspection = GetStartedInspectionInstance(johnDoe);
      var inspectionRepo = GetMockInspectionRepo(inspection);
      var testRepo = MockEmptyTestRepo();
      var employeeRepo = MockEmployeeRepo(johnDoe);

      var service = GetServiceInstance(inspectionRepo, testRepo, employeeRepo);

      // When
      Task AsyncTestDelegate() => service.CompleteInspectionAsync(inspection.Id, DateTime.Now, default(CancellationToken));

      // Then
      Assert.ThrowsAsync<BadRequestException>(AsyncTestDelegate);
    }

    [Test]
    public async Task ShouldCompleteCompletedInspection()
    {
      // Given
      Inspection inspection = GetFinishedInspectionInstance();

      // repository containing the started inspection...
      var inspectionRepo = GetMockInspectionRepo(inspection);
      var testRepo = MockMatchingTestRepo(inspection);
      var employeeRepo = MockMatchingEmployeeRepo(inspection);

      var service = GetServiceInstance(inspectionRepo, testRepo, employeeRepo);

      // When // Act
      await service.CompleteInspectionAsync(inspection.Id, DateTime.Now, default(CancellationToken));

      // Then // Assert
      // not exception is thrown;
    }

    [Test]
    public async Task ShouldBeginInspection_GivenInspectionNotExists()
    {
      // Given // Arrange
      var johnDoe = GetJohnDoeEmployee();
      Inspection inspection = GetStartedInspectionInstance(johnDoe);

      // repository containing no inspections...
      var mockRepo = GetMockInspectionRepo(null);
        
      var testRepo = MockEmptyTestRepo();
      var employeeRepo = MockEmployeeRepo(johnDoe);

      var service = GetServiceInstance(mockRepo, testRepo, employeeRepo);

      // When // Act
      await service.BeginInspectionAsync(inspection);

      // Then // Assert
      mockRepo.Received().Add(Arg.Is<Inspection>(e => e.Id != Guid.Empty));
      await mockRepo.Received().SaveChangesAsync();
    }

    [Test]
    public async Task ShouldAssignId_GivenInspectionDoesntExist()
    {
      // Given // Arrange
      var johnDoe = GetJohnDoeEmployee();

      var inspection = GetStartedInspectionInstance(johnDoe);

      // repository containing no inspections...
      var mockRepo = GetMockInspectionRepo(null);

      var testRepo = MockEmptyTestRepo();
      var employeeRepo = MockEmployeeRepo(johnDoe);

      var service = GetServiceInstance(mockRepo, testRepo, employeeRepo);

      // When // Act
      await service.BeginInspectionAsync(inspection);

      // Then // Assert
      mockRepo.Received().Add(Arg.Is<Inspection>(e => e.Id != Guid.Empty));
      await mockRepo.Received().SaveChangesAsync();

    }

    [Test]
    public async Task ShouldAssignDenormalizedIds_GivenInspectionDoesntExist()
    {
      // Given // Arrange
      var johnDoe = GetJohnDoeEmployee();

      var inspection = GetStartedInspectionInstance(johnDoe);

      // repository containing no inspections...
      var mockRepo = GetMockInspectionRepo(null);

      var testRepo = MockEmptyTestRepo();
      var employeeRepo = MockEmployeeRepo(johnDoe);

      var service = GetServiceInstance(mockRepo, testRepo, employeeRepo);

      // When // Act
      await service.BeginInspectionAsync(inspection);

      // Then // Assert
      Assert.That(inspection.DepartmentId, Is.EqualTo(johnDoe.DepartmentId));
      Assert.That(inspection.BranchOfficeId, Is.EqualTo(johnDoe.BranchOfficeId));
    }


    [Test]
    public void ShouldNotAddDuplicateInspections()
    {
      // Given // Arrange
      var johnDoe = GetJohnDoeEmployee();
      var inspection = GetStartedInspectionInstance(johnDoe);

      // repository containing the inspection with an ID assigned
      inspection.Id = Guid.NewGuid();
      var inspectionRepo = GetMockInspectionRepo(inspection);
      var testRepo = MockEmptyTestRepo();
      var employeeRepo = MockMatchingEmployeeRepo(inspection);

      var service = GetServiceInstance(inspectionRepo, testRepo, employeeRepo);

      // When // Act
      Task AsyncTestDelegate() => service.BeginInspectionAsync(inspection);

      // Then // Assert
      var ex = Assert.ThrowsAsync<ItemAlreadyExistsException>(AsyncTestDelegate);
      Assert.That(ex.Id.HasValue && inspection.Id == ex.Id.Value);
    }

    [Test]
    public void ShouldNotAddFinishedInspection()
    {
      // Given // Arrange
      var inspection = GetFinishedInspectionInstance();

      // repository containing no inspections...
      var inspectionRepo = GetMockInspectionRepo(null);
      var testRepo = MockEmptyTestRepo();
      var employeeRepo = MockMatchingEmployeeRepo(inspection);

      var service = GetServiceInstance(inspectionRepo, testRepo, employeeRepo);

      // When // Act
      Task AsyncTestDelegate() => service.BeginInspectionAsync(inspection);

      // Then // Assert
      Assert.ThrowsAsync<BadRequestException>(AsyncTestDelegate);
    }

    private Inspection GetFinishedInspectionInstance()
    {
      var johnDoe = GetJohnDoeEmployee();
      var inspection = GetStartedInspectionInstance(johnDoe);
      inspection.FinishTime = inspection.StartTime + TimeSpan.FromMinutes(5);
      return inspection;
    }

    private static Inspection GetStartedInspectionInstance(Employee employee)
    {
      return new Inspection()
      {
        Id = Guid.NewGuid(),
        StartTime = DateTime.Now - TimeSpan.FromMinutes(6),
        FinishTime = null,
        MethodSetId = "123",
        EmployeeId = employee.Id
      };
    }

    private Employee GetJohnDoeEmployee()
    {
      return new Employee
      {
        Id = Guid.NewGuid(),
        DepartmentId = Guid.NewGuid(),
        FirstName = "John",
        LastName = "Doe"
      };
    }

    private static IGuidKeyedRepository<Inspection> GetMockInspectionRepo(Inspection inspectionWithTest)
    {
      var mockRepo = Substitute.For<IGuidKeyedRepository<Inspection>>();
      mockRepo.SetupRepositoryWithEntity(inspectionWithTest);
      return mockRepo;
    }

    private static IGuidKeyedRepository<Test> MockMatchingTestRepo(Inspection inspectionWithTest)
    {
      var testRepository = Substitute.For<IGuidKeyedRepository<Test>>();
      testRepository.SetupRepositoryWithEntity(new Test { InspectionId = inspectionWithTest.Id });
      return testRepository;
    }

    private static IGuidKeyedRepository<Test> MockEmptyTestRepo()
    {
      var testRepository = Substitute.For<IGuidKeyedRepository<Test>>();
      testRepository.Query().Returns(new Test[] { }.AsEntityAsyncQueryable());
      return testRepository;
    }

    private static IGuidKeyedRepository<Employee> MockMatchingEmployeeRepo(Inspection inspectionWithTest)
    {
      var mockEmployeeRepo = Substitute.For<IGuidKeyedRepository<Employee>>();
      mockEmployeeRepo.SetupRepositoryWithEntity(new Employee
      {
        Id = inspectionWithTest.EmployeeId,
        DepartmentId = inspectionWithTest.DepartmentId,
        BranchOfficeId = inspectionWithTest.BranchOfficeId
      });
      return mockEmployeeRepo;
    }

    private static IGuidKeyedRepository<Employee> MockEmployeeRepo(Employee employee)
    {
      var mockEmployeeRepo = Substitute.For<IGuidKeyedRepository<Employee>>();
      mockEmployeeRepo.SetupRepositoryWithEntity(employee);
      return mockEmployeeRepo;
    }

  }
}

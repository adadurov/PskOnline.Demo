namespace PskOnline.Service.Test.Services
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  using NUnit.Framework;
  using NSubstitute;

  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.DAL.OrgStructure;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using System.Threading.Tasks;

  [TestFixture]
  class EmployeeService_Test
  {
    private IGuidKeyedRepository<BranchOffice> _branchRepo;
    private BranchOffice _branch;

    private IGuidKeyedRepository<Department> _departmentRepo;
    private Department _department;

    private IGuidKeyedRepository<Position> _positionRepo;
    private Position _position;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      _department = new Department
      {
        Name = "Department",
        Id = Guid.NewGuid()
      };
      _departmentRepo = Substitute.For<IGuidKeyedRepository<Department>>();

      _branch = new BranchOffice
      {
        Name = "Branch Office",
        Id = Guid.NewGuid()
      };
      _branchRepo = Substitute.For<IGuidKeyedRepository<BranchOffice>>();

      _position = new Position
      {
        Name = "Position",
        Id = Guid.NewGuid()
      };
      _positionRepo = Substitute.For<IGuidKeyedRepository<Position>>();

    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public async Task Create_Should_Assign_BranchOfficeId()
    {
      // Given
      // there are no employees in the repo...
      var employeeRepo = Substitute.For<IGuidKeyedRepository<Employee>>();
      employeeRepo.SetupRepositoryWithEntity(null);

      _branchRepo.SetupRepositoryWithEntity(_branch);
      _departmentRepo.SetupRepositoryWithEntity(_department);
      _positionRepo.SetupRepositoryWithEntity(_position);

      var employeeService = GetEmployeeService(_branchRepo, _departmentRepo, employeeRepo, _positionRepo);

      // When
      var johnDoe = new Employee
      {
        FirstName = "John",
        LastName = "Doe",
        DepartmentId = _department.Id,
        PositionId = _position.Id
      };
      await employeeService.AddAsync(johnDoe);

      // Then
      await _departmentRepo.Received().GetAsync(johnDoe.DepartmentId);

      employeeRepo.Received().Add(johnDoe);
      await employeeRepo.Received().SaveChangesAsync();
    }

    [Test]
    public void Create_Should_Fail_GivenEmployeeAlreadyExists()
    {
      // Given
      // employee repo already contains an employee
      var johnDoe = new Employee
      {
        Id = Guid.NewGuid(),
        FirstName = "John",
        LastName = "Doe",
        DepartmentId = _department.Id
      };
      var employeeRepo = Substitute.For<IGuidKeyedRepository<Employee>>();
      employeeRepo.SetupRepositoryWithEntity(johnDoe);

      _branchRepo.SetupRepositoryWithEntity(_branch);
      _departmentRepo.SetupRepositoryWithEntity(_department);


      var employeeService = GetEmployeeService(_branchRepo, _departmentRepo, employeeRepo, _positionRepo);

      // When
      AsyncTestDelegate action = async () => await employeeService.AddAsync(johnDoe);

      // Then
      Assert.ThrowsAsync<ConflictException>(action);
    }

    [Test]
    public async Task Update_Should_Update_BranchOfficeId()
    {
      // Given
      // there is an employees in the repo...
      var johnDoe = new Employee
      {
        FirstName = "John",
        LastName = "Doe",
        DepartmentId = _department.Id,
        PositionId = _position.Id
      };
      var employeeRepo = Substitute.For<IGuidKeyedRepository<Employee>>();
      employeeRepo.SetupRepositoryWithEntity(johnDoe);

      var newBranch = new BranchOffice
      {
        Id = Guid.NewGuid(),
        Name = "Some new branch office"
      };
      _branchRepo.SetupRepositoryWithEntity(newBranch);
      var newDepartment = new Department
      {
        Id = Guid.NewGuid(),
        Name = "Some New Department",
        BranchOfficeId = newBranch.Id
      };
      _departmentRepo.SetupRepositoryWithEntity(newDepartment);
      _positionRepo.SetupRepositoryWithEntity(_position);

      var employeeService = GetEmployeeService(_branchRepo, _departmentRepo, employeeRepo, _positionRepo);

      // When
      // the employee is update (moved to a new department within a new branch office)
      johnDoe.DepartmentId = newDepartment.Id;
      await employeeService.UpdateAsync(johnDoe);

      // Then
      // the employee's branch office is updated to the ID 
      // of the branch office that the new department belongs to
      Assert.That(johnDoe.BranchOfficeId, Is.EqualTo(newBranch.Id));
      // and the changes should have been saved
      employeeRepo.Received().Update(johnDoe);
      await employeeRepo.Received().SaveChangesAsync();
    }


    private IEmployeeService GetEmployeeService(
      IGuidKeyedRepository<BranchOffice> branchRepo, IGuidKeyedRepository<Department> deptRepo, IGuidKeyedRepository<Employee> repository,
      IGuidKeyedRepository<Position> positionRepo)
    {
      return new EmployeeService(
        branchRepo,
        deptRepo,
        positionRepo, 
        Substitute.For<ITenantEntityAccessChecker>(), 
        Substitute.For<IAccessScopeFilter>(),
        repository,
        Substitute.For<ILogger<EmployeeService>>()
        );
    }
  }
}

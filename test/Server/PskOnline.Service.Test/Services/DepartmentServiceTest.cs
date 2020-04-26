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

  [TestFixture]
  class DepartmentService_Test
  {
    private DepartmentService _serviceUnderTest;
    private IGuidKeyedRepository<BranchOffice> _branchOfficeRepository;
    private IGuidKeyedRepository<Department> _deptRepo;

    private BranchOffice _branch;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

       _branch = new BranchOffice
       {
         Name = "Branch Office",
         Id = Guid.NewGuid()
       };
      _branchOfficeRepository = Substitute.For<IGuidKeyedRepository<BranchOffice>>();
      _deptRepo = Substitute.For<IGuidKeyedRepository<Department>>();
      _deptRepo.Query().Returns((new Department[0]).AsEntityAsyncQueryable());

      _serviceUnderTest = new DepartmentService(
        _branchOfficeRepository,
        Substitute.For<ITenantEntityAccessChecker>(),
        Substitute.For<IAccessScopeFilter>(),
        _deptRepo,
        Substitute.For<ILogger<DepartmentService>>()
        );
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void Create_Should_Check_Access_To_Referred_BranchOffice()
    {
      // Given
      _branchOfficeRepository.SetupRepositoryWithEntity(_branch);

      // When
      var newDep = new Department
      {
        Name = "Operations department",
        BranchOfficeId = _branch.Id
      };
      _serviceUnderTest.AddAsync(newDep).Wait();

      // Then
      _branchOfficeRepository.Received().GetAsync(newDep.BranchOfficeId);
      _deptRepo.Received().Add(newDep);
      _deptRepo.Received().SaveChangesAsync();
    }

    [Test]
    public void Create_With_Duplicate_Name_Should_Throw_ItemAlreadyExistsException_Without_Id()
    {
      // Given
      var existingDep = new Department
      {
        Id = Guid.NewGuid(), // any entity in repository shall have an Id
        Name = "Operations department",
        BranchOfficeId = Guid.NewGuid()
      };
      _deptRepo.GetSingleOrDefaultAsync(
        Arg.Any<Expression<Func<Department, bool>>>()).Returns(existingDep);
      _deptRepo.Query().Returns( (new [] { existingDep }).AsEntityAsyncQueryable().AsQueryable() );

      // When
      var newDep = new Department
      {
        Name = "Operations department",
        BranchOfficeId = existingDep.BranchOfficeId
      };

      AsyncTestDelegate action = async () => await _serviceUnderTest.AddAsync(newDep);

      // Then
      var ex = Assert.ThrowsAsync<BadRequestException>(action);
      _deptRepo.DidNotReceive().SaveChangesAsync();
    }

    [Test]
    public void Update_Should_Check_Access_To_Referred_BranchOffice()
    {
      // Given
      _branchOfficeRepository.SetupRepositoryWithEntity(_branch);

      var newDep = new Department
      {
        Id = Guid.NewGuid(),
        Name = "Operations Department",
        BranchOfficeId = _branch.Id
      };
      _deptRepo.SetupRepositoryWithEntity(newDep);

      // When
      _serviceUnderTest.UpdateAsync(newDep).Wait();

      // Then
      _branchOfficeRepository.Received().GetAsync(newDep.BranchOfficeId);
      _deptRepo.Received().Update(newDep);
      _deptRepo.Received().SaveChangesAsync();
    }
  }
}

namespace PskOnline.Service.Test.Services
{
  using System;
  using NUnit.Framework;
  using NSubstitute;

  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Repository;
  using Microsoft.Extensions.Logging;
  using System.Linq.Expressions;
  using PskOnline.Server.DAL.OrgStructure;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Exceptions;

  [TestFixture]
  class PositionService_Test
  {
    private PositionService _serviceUnderTest;
    private IGuidKeyedRepository<Position> _repository;

    private IGuidKeyedRepository<BranchOffice> _branchOfficeRepo;
    private BranchOffice _branch;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      _branchOfficeRepo = Substitute.For<IGuidKeyedRepository<BranchOffice>>();
      _branch = new BranchOffice
      {
        Name = "Branch office",
        Id = Guid.NewGuid()
      };
      _branchOfficeRepo.SetupRepositoryWithEntity(_branch);

      _repository = Substitute.For<IGuidKeyedRepository<Position>>();
      _repository.Query().Returns((new Position[0]).AsEntityAsyncQueryable());
      _serviceUnderTest = new PositionService(
        _branchOfficeRepo,
        Substitute.For<ITenantEntityAccessChecker>(),
        Substitute.For<IAccessScopeFilter>(),
        _repository,
        Substitute.For<ILogger<PositionService>>()
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
      var newPos = new Position {
        Name = "Electrical Engineer",
        BranchOfficeId = _branch.Id
      };

      // When
      _serviceUnderTest.AddAsync(newPos).Wait();

      // Then
      _branchOfficeRepo.Received().GetAsync(newPos.BranchOfficeId.Value);
      _repository.Received().Add(newPos);
      _repository.Received().SaveChangesAsync();
    }

    [Test]
    public void Update_Should_Check_Access_To_Referred_BranchOffice()
    {
      // Given
      var newPos = new Position {
        Id = Guid.NewGuid(),
        Name = "Electrical Engineer",
        BranchOfficeId = _branch.Id
      };
      _repository.SetupRepositoryWithEntity(newPos);

      // When
      _serviceUnderTest.UpdateAsync(newPos).Wait();

      // Then
      _branchOfficeRepo.Received().GetAsync(newPos.BranchOfficeId.Value);
      _repository.Received().Update(newPos);
      _repository.Received().SaveChangesAsync();
    }

    [Test]
    public void Create_With_Duplicate_Name_Should_Throw_ItemAlreadyExistsException_Without_Id()
    {
      // Given
      // an entity in the repository
      var existingPos = new Position
      {
        Id = Guid.NewGuid(),
        Name = "Chief Electrician",
        BranchOfficeId = _branch.Id,
      };
      _repository.SetupRepositoryWithEntity(existingPos);

      // When
      var newPos = new Position
      {
        Name = "Chief Electrician",
        BranchOfficeId = existingPos.BranchOfficeId,
      };

      AsyncTestDelegate action = async () => await _serviceUnderTest.AddAsync(newPos);

      // Then
      var ex = Assert.ThrowsAsync<BadRequestException>(action);
      _repository.DidNotReceive().SaveChangesAsync();
    }


  }
}

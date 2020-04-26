namespace PskOnline.Service.Test.Services
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  using Microsoft.Extensions.Logging;
  using NUnit.Framework;
  using NSubstitute;

  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.DAL.OrgStructure;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Exceptions;

  [TestFixture]
  class BranchOfficeService_Test
  {
    private BranchOfficeService _serviceUnderTest;
    private IGuidKeyedRepository<BranchOffice> _repository;

    [SetUp]
    public void Setup()
    {
      LogHelper.ConfigureConsoleLogger();

      _repository = Substitute.For<IGuidKeyedRepository<BranchOffice>>();
      _serviceUnderTest = new BranchOfficeService(
        Substitute.For<ITenantEntityAccessChecker>(),
        Substitute.For<IAccessScopeFilter>(),
        _repository,
        Substitute.For<ILogger<BranchOfficeService>>()
        );
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    public void AddAsyncShouldThrowBadRequestException_GivenDuplicateBranchOfficeName()
    {
      // Given
      // Repository containing item named "Zeyskaya GES"
      var newBranch = new BranchOffice
      {
        Id = Guid.NewGuid(), // item in repository must have guid
        Name = "Zeyskaya GES",
      };
      _repository.GetSingleOrDefaultAsync(
        Arg.Any<Expression<Func<BranchOffice, bool>>>()).Returns(newBranch);
      _repository.Query().Returns((new[] { newBranch }).AsEntityAsyncQueryable().AsQueryable());

      var newBranch2 = new BranchOffice
      {
        Name = "Zeyskaya GES",
      };

      // When
      async System.Threading.Tasks.Task action() => await _serviceUnderTest.AddAsync(newBranch2);

      // Then
      var ex = Assert.ThrowsAsync<BadRequestException>(action);
      _repository.DidNotReceive();
      _repository.DidNotReceive().SaveChangesAsync();
    }

    [Test]
    public void AddAsyncShouldThrowConflicException_GivenDuplicateBranchOfficeId()
    {
      // Given
      // Repository containing item named "Zeyskaya GES"
      var newBranch = new BranchOffice
      {
        Id = Guid.NewGuid(), // item in repository must have guid
        Name = "Zeyskaya GES",
      };
      _repository.GetSingleOrDefaultAsync(
        Arg.Any<Expression<Func<BranchOffice, bool>>>()).Returns(newBranch);
      _repository.Query().Returns((new[] { newBranch }).AsEntityAsyncQueryable().AsQueryable());

      var newBranch2 = new BranchOffice
      {
        Id = newBranch.Id,
        Name = "Zeyskaya GES",
      };

      // When
      async System.Threading.Tasks.Task action() => await _serviceUnderTest.AddAsync(newBranch2);

      // Then
      var ex = Assert.ThrowsAsync<ConflictException>(action);
      _repository.DidNotReceive();
      _repository.DidNotReceive().SaveChangesAsync();
    }

  }
}

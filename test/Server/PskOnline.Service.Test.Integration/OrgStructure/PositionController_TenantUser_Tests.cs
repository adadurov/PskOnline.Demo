namespace PskOnline.Service.Test.Integration.OrgStructure
{
  using System;
  using System.Linq;
  using System.Net.Http;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Mvc.Testing;
  using NUnit.Framework;

  using PskOnline.Client.Api;
  using PskOnline.Client.Api.Authority;
  using PskOnline.Client.Api.Organization;
  using PskOnline.Components.Log;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Service.Test.Integration.TestData;

  [TestFixture(
    Author = "Adadurov",
    Description = "Validates various scenarios related to Positions. " +
                  "Notice that this test fixture is initialized only once!")]
  public class PositionController_TenantUser_Tests : IDisposable
  {
    DefaultWebApplicationFactory _app;
    HttpClient _httpClient;
    IApiClient _apiClient;

    TenantContainer _gryffindorHouse;

    TenantContainer _slytherinHouse;

    UserDto _siteAdminUser;

    Guid _nonExistingTenantId = Guid.NewGuid();

    [SetUp]
    public async Task SetUp()
    {
      await InitOnce();

      await _apiClient.AsGryffindorAdminAsync(_httpClient);
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

      var options = new WebApplicationFactoryClientOptions()
      {
        AllowAutoRedirect = false
      };
      _httpClient = _app.CreateClient(options);

      _apiClient = new ApiClient(_httpClient, _app.GetLogger<ApiClient>());

      await _apiClient.AsSiteAdminAsync(_httpClient);
      _siteAdminUser = await _apiClient.GetCurrentUserAsync();

      _gryffindorHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.GryffindorHouse);

      await _apiClient.AsSiteAdminAsync(_httpClient);
      _slytherinHouse = await TestTenants.CreateTenantWithRolesAndUsers(_apiClient, _httpClient, TestTenants.SlytherinHouse);

      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _slytherinHouse).Wait();

      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      TestBranchOffice.SeedDefaultBranchOffice(_apiClient, _httpClient, _gryffindorHouse).Wait();
    }

    [Test]
    public async Task CreateAndReadPosition_ShouldSucceed()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Position #100",
      };

      // When
      // the user posts a new position...
      var newId = await _apiClient.PostPositionAsync(positionDto);

      // Then
      // the request should succeed

      // And the following 'read' request should succeed
      var createdPositionDto = await _apiClient.GetPositionAsync(newId);

      Assert.That(createdPositionDto.Name, Is.EqualTo(positionDto.Name));
    }

    [Test]
    public async Task CreatePosition_ShouldFail_GivenDuplicateName()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // Position DTO
      var positionDto = new PositionDto
      {
        Name = "Position #1",
//        BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString(),
      };
      var createFirstPositionResponse = await _apiClient.PostPositionAsync(positionDto);

      // When
      // A position with the same name is posted again
      positionDto.Id = null;
      AsyncTestDelegate action = async () => await _apiClient.PostPositionAsync(positionDto);

      // Then
      // the request should succeed
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    [Ignore("Until Position has reference to BranchOffice")]
    public void CreatePosition_ShouldFail_GivenMissingReferenceToBranchOffice()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)

      // When
      // The user tries to create a position without reference to Branch Office
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Position Without Reference to Branch Office",
        // BranchOfficeId = _gryffindorHouse.BranchOffice_One.Id.ToString(),
      };

      AsyncTestDelegate action = async () => await _apiClient.PostPositionAsync(positionDto);

      // Then
      // the request should fail with 'BadRequest' status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    [Ignore("Until Position has reference to BranchOffice")]
    public void CreatePosition_ShouldFail_GivenReferenceToBranchOfficeInOtherTenant()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)

      // When
      // The user tries to create a position without reference to Branch Office
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Position With Reference to Other Tenant Branch Office",
      };

      AsyncTestDelegate action = async () => await _apiClient.PostPositionAsync(positionDto);

      // Then
      // the request should fail with 'BadRequest' status code
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public async Task CreatePosition_ShouldFail_GivenDuplicateId()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created a Position with name "Position #11"
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Position #11",
      };
      var firstId = await _apiClient.PostPositionAsync(positionDto);

      // When
      // The user tries to create another Position with the same Id
      // and a different name
      positionDto.Name += ".001";
      AsyncTestDelegate action = async () => await _apiClient.PostPositionAsync(positionDto);

      // Then
      // the request should fail with 'Conflict' status code
      Assert.ThrowsAsync<ConflictException>(action);
    }

    [Test]
    public async Task CreatePosition_ShouldSucceed_GivenSameNames_InTwoTenants()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      var positionDto = new PositionDto
      {
        Name = "Position #101"
      };
      // created a tenant with the name "Position #100"
      var newGryffindorPositionId = await _apiClient.PostPositionAsync(positionDto);

      // switch to tenant #2
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      // When
      // user from different tenant creates a Position with the name
      // duplicating name of a Position in a different tenant...
      positionDto.Id = null;
      var newSlytherinPositionId = await _apiClient.PostPositionAsync(positionDto);

      // Then
      // the request should succeed

      Assert.That(newSlytherinPositionId, Is.Not.EqualTo(newGryffindorPositionId));
    }

    [Test]
    public async Task GetAllPositions_ShouldSucceed()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created 2 positions just created in Gryffindor House
      // and 1 position just created in Slytherin House
      var positionDto1 = new PositionDto
      {
        Name = "Position #1001",
      };
      var firstPositionId = await _apiClient.PostPositionAsync(positionDto1);

      var positionDto = new PositionDto
      {
        Name = "Position #1002",
      };
      var secondPositionId = await _apiClient.PostPositionAsync(positionDto);

      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      var slytherinPositionDto = new PositionDto
      {
        Name = "Slytherin Position #1002",
//        BranchOfficeId = _slytherinHouse.BranchOffice_One.Id.ToString()
      };
      var slytherinPositionId = await _apiClient.PostPositionAsync(slytherinPositionDto);

      // When
      // Gryffindor user requests all positions
      await _apiClient.AsGryffindorAdminAsync(_httpClient);
      var allGryffindorPositions = await _apiClient.GetPositionsAsync();

      // Then
      // the request should succeed and return more than 1 positions
      Assert.That(allGryffindorPositions.Count(), Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task GetPosition_ShouldFail_GivenWrongTenant()//
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // Position DTO
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = "Position To Be Forbidden",
      };

      var newId = await _apiClient.PostPositionAsync(positionDto);

      // When
      // Slytherin user tries to read the new Branch office...
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      AsyncTestDelegate action = async () => await _apiClient.GetPositionAsync(newId);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task UpdatePosition_ShouldSucceed()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Position named Position #5
      var bo2Name = "Position #5";
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
      };

      var bo2Guid = await _apiClient.PostPositionAsync(positionDto);
      positionDto.Id = bo2Guid.ToString();

      positionDto.Name = "Position #5.1";

      // When
      // the user attempts to rename Position #5 to Position #5.1
      await _apiClient.PutPositionAsync(positionDto);

      // Then
      // the request should succeed   
    }

    [Test]
    public async Task UpdatePosition_ShouldFail_GivenPositionInOtherTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Position named "Position To Fail Update"
      var positionDto = new PositionDto
      {
        Name = "Position To Fail Update",
      };

      var newId = await _apiClient.PostPositionAsync(positionDto);

      // When
      // a user from a different tenant attempts to update the Position
      await _apiClient.AsSlytherinAdminAsync(_httpClient);
      positionDto.Name += " updated";
      AsyncTestDelegate action = async () => await _apiClient.PutPositionAsync(positionDto);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task UpdatePosition_ShouldFail_GivenDuplicateName()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // created 2 branch offices: "Position #2" & "Position #3"
      var pos2name = "Position #2";
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = pos2name,
      };

      var positionDtoResponse = await _apiClient.PostPositionAsync(positionDto);

      positionDto.Name = "Position #3";
      positionDto.Id = Guid.NewGuid().ToString();
      var pos3guid = await _apiClient.PostPositionAsync(positionDto);

      positionDto.Id = pos3guid.ToString();

      positionDto.Name = pos2name;

      // When
      // user attempts to rename Position #3 to Position #2
      AsyncTestDelegate action = async () => await _apiClient.PutPositionAsync(positionDto);

      // Then
      // the request should fail
      Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Test]
    public async Task DeletePosition_ShouldSucceed_GivenPositionInUsersTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Position named Position #5
      var position2name = "Position #500";
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = position2name,
      };

      await _apiClient.PostPositionAsync(positionDto);

      // When
      // the user attempts to delete the Position
      await _apiClient.DeletePositionAsync(positionDto.Id);

      // Then
      // the request should succeed
      // and the following request to get the position should fail
      Assert.ThrowsAsync<ItemNotFoundException>(
        async () => await _apiClient.GetPositionAsync(positionDto.Id)
        );
    }

    [Test]
    public async Task DeletePosition_ShouldFail_GivenPositionInOtherTenant()
    {
      // Given
      // user authenticated in tenant #1 (Gryffindor House)
      // and created Position named Position #50
      var bo2Name = "Position #50";
      var positionDto = new PositionDto
      {
        Id = Guid.NewGuid().ToString(),
        Name = bo2Name,
      };

      var createPositionResponse = await _apiClient.PostPositionAsync(positionDto);

      // When
      // a user from a different tenant attempts to delete the Branch Office
      await _apiClient.AsSlytherinAdminAsync(_httpClient);

      AsyncTestDelegate action = async () => await _apiClient.DeletePositionAsync(positionDto.Id);

      // Then
      // the request should fail
      Assert.ThrowsAsync<UnauthorizedAccessException>(action);
    }
  }
}

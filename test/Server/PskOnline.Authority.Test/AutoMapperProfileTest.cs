namespace PskOnline.Authority.Test
{
  using System;
  using AutoMapper;
  using NUnit.Framework;
  using FluentAssertions;

  using PskOnline.Server.Authority;
  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.Authority.ObjectModel;

  [TestFixture]
  public class AutoMapperProfileTest
  {
    IMapper _mapper;

    [SetUp]
    public void SetUp()
    {
      var cfg = new AutoMapperConfig();

      _mapper = cfg.CreateMapper();
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    public void UserDto_To_User_And_Back_Should_Succeed()
    {
      var userViewModel = new UserDto()
      {
        Id = Guid.NewGuid().ToString(),
        UserName = "Ivan.Ivanov",
        IsEnabled = true,
        Email = "someone@somedomain.net",
        PhoneNumber = "+999.222.123.45.67",
        WebUiPreferences = Guid.NewGuid().ToString(),

        FirstName = "Иван",
        Patronymic = "Иванович",
        LastName = "Иванов",

        DepartmentId = Guid.NewGuid().ToString(),
        TenantId = Guid.NewGuid().ToString(),
        BranchOfficeId = Guid.NewGuid().ToString()
      };
      var user = _mapper.Map<ApplicationUser>(userViewModel);

      var newUserViewModel = _mapper.Map<UserDto>(user);

      newUserViewModel.Should().BeEquivalentTo(userViewModel);
    }

  }
}


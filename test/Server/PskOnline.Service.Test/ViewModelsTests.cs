namespace PskOnline.Service.Test
{
  using System;
  using AutoMapper;
  using FluentAssertions;
  using NUnit.Framework;

  using PskOnline.Server.Authority.API.Dto;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service.ViewModels;

  [TestFixture]
  public class ViewModelsTests
  {
    [SetUp]
    public void SetUp()
    {
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    public void UserToUserViewModel()
    {
    }

    [Test]
    public void TenantAdminModel_ToAppUser()
    {
      var createTenantDto = new TenantCreateDto
      {
        TenantDetails = new TenantDto
        {
          Name = "Demo tenant 1",
          PrimaryContact = new ContactInfoDto
          {
            FullName = "John Doe"
          },
          ServiceDetails = new ServiceDetailsDto
          {
            ServiceExpireDate = DateTime.Now.Date + TimeSpan.FromDays(366)
          }
        },
        AdminUserDetails = new TenantCreateAdminDto
        {
          FirstName = "Ivanov",
          LastName = "Konstantin",
          Patronymic = "Mikhaylovich",
          UserName = "ivkomikh",
          Email = "ivkomikh@mail.to",
          NewPassword = "Qwerty123$"
        }
      };
      var user = Mapper.Map<UserDto>(createTenantDto.AdminUserDetails);

    }

    [Test]
    public void PositionViewModelToPosition()
    {
      var positionViewModel = new PositionDto()
      {
        Id = Guid.NewGuid().ToString(),
        Name = "NSS",
        //        BranchOfficeId = Guid.NewGuid().ToString()
      };

      var position = Mapper.Map<Position>(positionViewModel);

      var newPositionViewModel = Mapper.Map<PositionDto>(position);

      newPositionViewModel.Should().BeEquivalentTo(positionViewModel);
    }

    [Test]
    public void PositionViewModel_Partial_To_Position()
    {
      var positionViewModel = new PositionDto()
      {
        Id = Guid.NewGuid().ToString(),
        Name = "NSS",
        //        BranchOfficeId = Guid.NewGuid().ToString()
      };

      var position = Mapper.Map<Position>(positionViewModel);

      var newPositionViewModel = Mapper.Map<PositionDto>(position);

      newPositionViewModel.Should().BeEquivalentTo(positionViewModel);
    }

  }
}


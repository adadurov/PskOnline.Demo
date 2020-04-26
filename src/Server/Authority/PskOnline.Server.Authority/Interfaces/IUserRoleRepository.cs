namespace PskOnline.Server.Authority.Interfaces
{
  using System;
  using System.Linq;

  using Microsoft.AspNetCore.Identity;

  public interface IUserRoleRepository
  {
    IQueryable<IdentityUserRole<Guid>> Query();
  }
}

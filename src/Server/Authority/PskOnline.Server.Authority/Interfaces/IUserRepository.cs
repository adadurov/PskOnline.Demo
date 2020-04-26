namespace PskOnline.Server.Authority.Interfaces
{
  using PskOnline.Server.Authority.ObjectModel;
  using System.Linq;

  public interface IUserRepository
  {
    IQueryable<ApplicationUser> Query();
  }
}

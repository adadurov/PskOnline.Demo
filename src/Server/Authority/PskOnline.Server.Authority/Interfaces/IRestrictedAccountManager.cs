using System.Threading.Tasks;

namespace PskOnline.Server.Authority.Interfaces
{
  /// <summary>
  /// used to mark account manager implementations to be used at runtime
  /// (with access checks)
  /// </summary>
  public interface IRestrictedAccountManager : IAccountManager
  {
  }
}

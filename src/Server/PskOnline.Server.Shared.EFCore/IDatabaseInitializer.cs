namespace PskOnline.Server.Shared.EFCore
{
  using System.Threading.Tasks;

  /// <summary>
  /// Migrates and initializes the corresponding component's database
  /// </summary>
  public interface IDatabaseInitializer
  {
    Task SeedAsync();
  }
}

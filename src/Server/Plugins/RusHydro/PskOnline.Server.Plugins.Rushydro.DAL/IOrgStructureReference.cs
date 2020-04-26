namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using PskOnline.Server.ObjectModel;
  using System;
  using System.Threading.Tasks;

  public interface IOrgStructureReference
  {
    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns>non-null, if found; otherwise null</returns>
    Task<Employee> GetEmployeeAsync(Guid id);

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns>non-null, if found; otherwise null</returns>
    Task<BranchOffice> GetBranchOfficeAsync(Guid id);

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns>non-null, if found; otherwise null</returns>
    Task<Department> GetDepartmentAsync(Guid id);

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns>non-null, if found; otherwise null</returns>
    Task<Position> GetPositionAsync(Guid id);
  }
}

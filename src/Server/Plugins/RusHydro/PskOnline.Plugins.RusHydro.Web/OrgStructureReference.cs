namespace PskOnline.Server.Plugins.RusHydro.Web
{
  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Plugins.RusHydro.DAL;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Service;
  using System;
  using System.Threading.Tasks;

  public class OrgStructureReference : IOrgStructureReference
  {
    private readonly IEmployeeService _employeeService;
    private readonly IService<BranchOffice> _branchOfficeService;
    private readonly IService<Department> _departmentService;
    private readonly IService<Position> _positionService;

    public OrgStructureReference(IEmployeeService employeeService,
                                 IService<BranchOffice> branchOfficeService,
                                 IService<Department> departmentService,
                                 IService<Position> positionService)
    {
      _employeeService = employeeService;
      _branchOfficeService = branchOfficeService;
      _departmentService = departmentService;
      _positionService = positionService;
    }

    public async Task<BranchOffice> GetBranchOfficeAsync(Guid id)
    {
      try
      {
        return await _branchOfficeService.GetAsync(id);
      }
      catch( ItemNotFoundException )
      {
        return null;
      }
    }

    public async Task<Department> GetDepartmentAsync(Guid id)
    {
      try
      {
        return await _departmentService.GetAsync(id);
      }
      catch (ItemNotFoundException)
      {
        return null;
      }
    }

    public async Task<Employee> GetEmployeeAsync(Guid id)
    {
      try
      {
        return await _employeeService.GetAsync(id);
      }
      catch (ItemNotFoundException)
      {
        return null;
      }
    }

    public async Task<Position> GetPositionAsync(Guid id)
    {
      try
      {
        return await _positionService.GetAsync(id);
      }
      catch (ItemNotFoundException)
      {
        return null;
      }
    }
  }
}

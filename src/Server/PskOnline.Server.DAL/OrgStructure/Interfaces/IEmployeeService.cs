namespace PskOnline.Server.DAL.OrgStructure.Interfaces
{
  using System.Collections.Generic;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Service;

  public interface IEmployeeService : IService<Employee>
  {
    IEnumerable<Employee> GetEmployeesInDepartment(Department department);
  }
}

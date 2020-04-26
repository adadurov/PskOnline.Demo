namespace PskOnline.Server.Plugins.RusHydro.ObjectModel
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [ComplexType]
  public class Employee
  {
    public Employee()
    {
    }

    public Employee(Employee source)
    {
      Id = source.Id;
      FullName = source.FullName;
      BranchOfficeName = source.BranchOfficeName;
      BranchOfficeId = source.BranchOfficeId;
      DepartmentName = source.DepartmentName;
      DepartmentId = source.DepartmentId;
      PositionName = source.PositionName;
      PositionId = source.PositionId;
      ExternalId = source.ExternalId;
    }

    public Guid Id { get; set; }

    public string ExternalId { get; set; }

    [MaxLength(256)]
    public string FullName { get; set; }

    public Guid BranchOfficeId { get; set; }

    [MaxLength(256)]
    public string BranchOfficeName { get; set; }

    public Guid DepartmentId { get; set; }

    [MaxLength(256)]
    public string DepartmentName { get; set; }

    public Guid PositionId{ get; set; }

    [MaxLength(256)]
    public string PositionName { get; set; }
  }

}

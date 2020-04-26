namespace PskOnline.Methods.ObjectModel
{
  using System;

  using PskOnline.Methods.ObjectModel.Attributes;

  public class Patient
  {
    public Patient() { }

    public Patient(Patient src)
    {
      var p = src ?? throw new ArgumentNullException();
      Id = p.Id;
      Gender = p.Gender;
      BirthDate = p.BirthDate;
      Name = p.Name;
      BranchOfficeName = p.BranchOfficeName;
      BranchOfficeId = p.BranchOfficeId;
      DepartmentName = p.DepartmentName;
      DepartmentId = p.DepartmentId;
      PositionName = p.PositionName;
      PositionId = p.PositionId;
    }

    [Exportable(2000)]
    public string Id { get; set; }

    [Exportable(1900)]
    public string Name { get; set; }

    [Exportable(1800)]
    public Gender Gender { get; set; }

    [Exportable(1700)]
    public DateTime BirthDate { get; set; }

    public string BranchOfficeId { get; set; }

    [Exportable(1600)]
    public string BranchOfficeName { get; set; }

    public string DepartmentId { get; set; }

    [Exportable(1500)]
    public string DepartmentName { get; set; }

    public string PositionId { get; set; }

    [Exportable(1400)]
    public string PositionName { get; set; }

  }
}

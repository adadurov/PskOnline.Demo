namespace PskOnline.Server.Shared.Roles
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

  using PskOnline.Server.Shared.Permissions;

  public class RoleSpecification
  {
      public RoleSpecification(
        string guid,
        string name,
        string description,
        IEnumerable<IApplicationPermission> permissions
        )
    {
      Id = Guid.Parse(guid);
      Name = name;
      Description = description;
      Permissions = new List<IApplicationPermission>();
      Permissions.AddRange(permissions);
    }

    public Guid Id { get; set; }

    /// <summary>
    /// this one is visible to the user
    /// </summary>
    public string Name { get; set; }

    public string Description { get; set; }

    public List<IApplicationPermission> Permissions { get; set; }
  }
}

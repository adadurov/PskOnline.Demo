namespace PskOnline.Server.Authority
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.ObjectModel;

  /// <summary>
  /// if this seems to break, check this out:
  /// https://github.com/aspnet/Identity/issues/1112
  /// </summary>
  public class MulititenantRoleValidator : RoleValidator<ApplicationRole>
  {
    public MulititenantRoleValidator(
      IdentityErrorDescriber errors = null)
      : base(errors)
    {
      Describer = errors ?? new IdentityErrorDescriber();
    }

    private IdentityErrorDescriber Describer { get; set; }

    public override async Task<IdentityResult> ValidateAsync(
      RoleManager<ApplicationRole> manager, ApplicationRole role)
    {
      if (manager == null)
      {
        throw new ArgumentNullException(nameof(manager));
      }
      if (role == null)
      {
        throw new ArgumentNullException(nameof(role));
      }
      var errors = new List<IdentityError>();
      await ValidateRoleName(manager, role, errors);
      if (errors.Count > 0)
      {
        return IdentityResult.Failed(errors.ToArray());
      }
      return IdentityResult.Success;
    }

    private async Task ValidateRoleName(
      RoleManager<ApplicationRole> manager, ApplicationRole role, ICollection<IdentityError> errors)
    {
      var roleName = await manager.GetRoleNameAsync(role);
      if (string.IsNullOrWhiteSpace(roleName))
      {
        errors.Add(Describer.InvalidRoleName(roleName));
      }
      else
      {
        var newNormalizedName = manager.NormalizeKey(role.Name);
        var existingRole = await manager.Roles.FirstOrDefaultAsync(
          r => r.TenantId == role.TenantId &&
          r.NormalizedName == newNormalizedName);

        if (existingRole != null &&
            !string.Equals(await manager.GetRoleIdAsync(existingRole), await manager.GetRoleIdAsync(role)))
        {
          errors.Add(Describer.DuplicateRoleName(roleName));
        }
      }
    }

  }
}

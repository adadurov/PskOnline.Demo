﻿namespace PskOnline.Server.Authority
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using PskOnline.Server.Authority.ObjectModel;

  /// <summary>
  /// Implementation is based on
  /// https://github.com/aspnet/Identity/blob/release/2.1/src/Core/UserValidator.cs
  /// </summary>
  /// <typeparam name="TUser"></typeparam>
  public class MultitenantUserValidator<TUser> : UserValidator<TUser> where TUser : ApplicationUser
  {
    /// <summary>
    /// Creates a new instance of <see cref="UserValidator{TUser}"/>/
    /// </summary>
    /// <param name="errors">The <see cref="IdentityErrorDescriber"/> used to provider error messages.</param>
    public MultitenantUserValidator(IdentityErrorDescriber errors = null)
    {
      Describer = errors ?? new IdentityErrorDescriber();
    }

    /// <summary>
    /// Gets the <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator{TUser}"/>.
    /// </summary>
    /// <value>The <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator{TUser}"/>.</value>
    public IdentityErrorDescriber Describer { get; private set; }

    /// <summary>
    /// Validates the specified <paramref name="user"/> as an asynchronous operation.
    /// </summary>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user to validate.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the validation operation.</returns>
    public override async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
      if (manager == null)
      {
        throw new ArgumentNullException(nameof(manager));
      }
      if (user == null)
      {
        throw new ArgumentNullException(nameof(user));
      }
      var errors = new List<IdentityError>();
      await ValidateUserName(manager, user, errors);
      await ValidateEmail(manager, user, errors);
      return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
    }

    private async Task ValidateUserName(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
    {
      var userName = await manager.GetUserNameAsync(user);
      if (string.IsNullOrWhiteSpace(userName))
      {
        errors.Add(Describer.InvalidUserName(userName));
      }
      else if (!string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
          userName.Any(c => !manager.Options.User.AllowedUserNameCharacters.Contains(c)))
      {
        errors.Add(Describer.InvalidUserName(userName));
      }
      else
      {
        Debug.Assert(manager is MultitenantUserManager<ApplicationUser>);
        var mgr = manager as MultitenantUserManager<ApplicationUser>;

        var owner = await mgr.FindByNameInTenantAsync(userName, user.TenantId);
        if (owner != null &&
            !string.Equals(await mgr.GetUserIdAsync(owner), await mgr.GetUserIdAsync(user)))
        {
          errors.Add(Describer.DuplicateUserName(userName));
        }
      }
    }

    // make sure email is not empty, valid, and unique
    private async Task ValidateEmail(UserManager<TUser> manager, TUser user, List<IdentityError> errors)
    {
      var email = await manager.GetEmailAsync(user);
      if (string.IsNullOrWhiteSpace(email))
      {
        errors.Add(Describer.InvalidEmail(email));
        return;
      }
      if (!new EmailAddressAttribute().IsValid(email))
      {
        errors.Add(Describer.InvalidEmail(email));
        return;
      }
      if (manager.Options.User.RequireUniqueEmail)
      {

        var owner = await manager.FindByEmailAsync(email);
        if (owner != null &&
            !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
        {
          errors.Add(Describer.DuplicateEmail(email));
        }
      }
    }
  }
}

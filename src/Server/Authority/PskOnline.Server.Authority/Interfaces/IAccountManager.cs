namespace PskOnline.Server.Authority.Interfaces
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Http;
  using PskOnline.Server.Authority.ObjectModel;

  public enum SecurePasswordResetResult
  {
    Success,
    InvalidToken,
    PasswordTooSimple
  }

  public interface IAccountManager : API.IAccountService
  {
    IRoleService RoleService { get; }

    Task<(bool success, string[] errors)> CreateUserAsync(ApplicationUser user, IEnumerable<Guid> roleIds, string password);

    Task<Tuple<ApplicationUser, ApplicationRole[]>> GetUserAndRoleIdsAsync(Guid userId);
    Task<ApplicationUser> GetUserByEmailAsync(string email);
    Task<ApplicationUser> GetUserByIdAsync(string userId);
    Task<ApplicationUser> GetUserByUserNameAsync(string userName);
    Task<IList<Guid>> GetUserRoleIdsAsync(ApplicationUser user);
    Task<List<Tuple<ApplicationUser, ApplicationRole[]>>> GetUsersAndRoleIdsAsync(int page, int pageSize);

    Task UpdateUserAsync(ApplicationUser user);

    Task UpdateUserAsync(ApplicationUser user, IEnumerable<Guid> roles);

    Task<bool> TestCanDeleteUserAsync(string userId);
    Task<(bool success, string[] errors)> DeleteUserAsync(ApplicationUser user);

    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);

    /// <summary>
    /// Sends a password reset link to the specified <paramref name="userName"/>
    /// in the current tenant (identified by the slug in the URL for the currently executing reguest)
    /// </summary>
    /// <param name="userNameOrEmail">username or an email of the account that needs to reset their password</param>
    /// <param name="request">context for generating reset link (for URL scheme, host, port etc.)</param>
    /// <returns></returns>
    Task SendPasswordResetLinkAsync(string userNameOrEmail, HttpRequest request);

    /// <summary>
    /// Verifies and 
    /// Sets the password of the user to the specified value
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="token"></param>
    /// <param name="newPassword"></param>
    /// <returns>Tuple with 'success' flag and an optional array of errors.</returns>
    Task<(SecurePasswordResetResult status, string[] errors)> ResetPasswordUsingTokenAsync(string userName, string token, string newPassword);

    /// <summary>
    /// Sets the password for the user to the specified value.
    /// Requires that the current user has permission to update passwords for the specified user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newPassword"></param>
    /// <returns>Tuple with 'success' flag and an optional array of errors.</returns>
    Task<(bool success, string[] errors)> ResetPasswordAsync(ApplicationUser user, string newPassword);

    /// <summary>
    /// Changes the password for the current user to the specified value.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="currentPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns>Tuple with status and an optional array of errors.</returns>
    Task<(bool success, string[] errors)> UpdatePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
  }
}

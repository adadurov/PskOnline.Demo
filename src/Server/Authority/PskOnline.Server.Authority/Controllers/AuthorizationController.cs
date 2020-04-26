namespace PskOnline.Server.Authority.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;

  using AspNet.Security.OpenIdConnect.Server;
  using AspNet.Security.OpenIdConnect.Extensions;

  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Authentication;
  using AspNet.Security.OpenIdConnect.Primitives;
  using Microsoft.Extensions.Options;

  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Authority.API.Constants;
  using OpenIddict.Core;
  using System.Diagnostics;
  using OpenIddict.Server;

  // For more information on enabling Web API for empty projects,
  // visit http://go.microsoft.com/fwlink/?LinkID=397860

  public class AuthorizationController : Controller
  {
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly MultitenantUserManager<ApplicationUser> _userManager;

    private readonly OpenIddictApplicationManager<PskApplication> _applicationManager;

    public AuthorizationController(
        IOptions<IdentityOptions> identityOptions,
        SignInManager<ApplicationUser> signInManager,
        MultitenantUserManager<ApplicationUser> userManager,
        OpenIddictApplicationManager<PskApplication> applicationManager
      )
    {
      _identityOptions = identityOptions;
      _signInManager = signInManager;
      _userManager = userManager;
      _applicationManager = applicationManager;
    }

    /// <summary>
    /// Authenticate to the server with a user or a client credentials
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Upon success, returns OpenId tokens. Upon failure, returns error description</returns>
    /// <response code="400">If the request is not valid (e.g. invalid credentials, invalid scope etc.)</response>
    [HttpPost("~/connect/token")]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
    {
      Debug.Assert(request.IsTokenRequest(),
          "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
          "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

      if (request.IsPasswordGrantType())
      {
        var user = await _userManager.FindByEmailAsync(request.Username) ??
                   await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidGrant,
            ErrorDescription = "Please check that your email and password is correct"
          });
        }

        // Ensure the user is enabled.
        if (!user.IsEnabled)
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidGrant,
            ErrorDescription = "The specified user account is disabled"
          });
        }

        // Validate the username/password parameters and ensure the account is not locked out.
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        // Ensure the user is not already locked out.
        if (result.IsLockedOut)
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidGrant,
            ErrorDescription = "The specified user account has been suspended"
          });
        }

        // Reject the token request if two-factor authentication has been enabled by the user.
        if (result.RequiresTwoFactor)
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidGrant,
            ErrorDescription = "Invalid login procedure"
          });
        }

        // Ensure the user is allowed to sign in.
        if (result.IsNotAllowed)
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidGrant,
            ErrorDescription = "The specified user is not allowed to sign in"
          });
        }

        if (!result.Succeeded)
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidGrant,
            ErrorDescription = "Please check that your email and password is correct"
          });
        }

        // Create a new authentication ticket.
        var ticket = await CreateTicketAsync(request, user);

        return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
      }
      else if (request.IsRefreshTokenGrantType())
      {
        // Retrieve the claims principal stored in the refresh token.
        var info = await HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

        // Retrieve the user profile corresponding to the refresh token.
        // Note: if you want to automatically invalidate the refresh token
        // when the user password/roles change, use the following line instead:
        // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
        var user = await _userManager.GetUserAsync(info.Principal);
        if (user == null)
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidGrant,
            ErrorDescription = "The refresh token is no longer valid"
          });
        }

        // Ensure the user is still allowed to sign in.
        if (!await _signInManager.CanSignInAsync(user))
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidGrant,
            ErrorDescription = "The user is no longer allowed to sign in"
          });
        }

        // Create a new authentication ticket, but reuse the properties stored
        // in the refresh token, including the scopes originally granted.
        var ticket = await CreateTicketAsync(request, user);

        return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
      }
      else if (request.IsClientCredentialsGrantType())
      {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
        if (application == null)
        {
          return BadRequest(new OpenIdConnectResponse
          {
            Error = OpenIdConnectConstants.Errors.InvalidClient,
            ErrorDescription = "The client application was not found in the database."
          });
        }

        var ticket = CreateWorkplaceSignInTicket(request, application);
        return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
      }
      return BadRequest(new OpenIdConnectResponse
      {
        Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
        ErrorDescription = "The specified grant type is not supported"
      });
    }

    private AuthenticationTicket CreateWorkplaceSignInTicket(OpenIdConnectRequest request, PskApplication application)
    {
      // Create a new ClaimsIdentity containing the claims that
      // will be used to create an id_token, a token or a code.
      var identity = new ClaimsIdentity(
          OpenIddictServerDefaults.AuthenticationScheme,
          OpenIdConnectConstants.Claims.Name,
          OpenIdConnectConstants.Claims.Role);

      // Use the client_id as the subject identifier.
      identity.AddClaim(OpenIdConnectConstants.Claims.Subject, application.ClientId,
          OpenIdConnectConstants.Destinations.AccessToken,
          OpenIdConnectConstants.Destinations.IdentityToken);

      identity.AddClaim(OpenIdConnectConstants.Claims.Name, application.DisplayName,
          OpenIdConnectConstants.Destinations.AccessToken,
          OpenIdConnectConstants.Destinations.IdentityToken);

      var principal = new ClaimsPrincipal(identity);

      principal.AddUserTenantAndOrgStructureClaims(application.TenantId, application.BranchOfficeId, application.DepartmentId);

      // Create a new authentication ticket holding the user identity.
      var ticket = new AuthenticationTicket(
        principal, new AuthenticationProperties(),
        OpenIdConnectServerDefaults.AuthenticationScheme);

      var scopes = request.GetScopes();
      ticket.SetScopes(scopes);

      identity.AddClaim(
        CustomClaimTypes.TenantId,
        application.TenantId,
        OpenIdConnectConstants.Destinations.IdentityToken, OpenIdConnectConstants.Destinations.AccessToken);

      // a 'tenant auditor' workplace application
      // doesn't have a 'branch office id' claim
      if (!string.IsNullOrEmpty(application.BranchOfficeId))
      {
        identity.AddClaim(
        CustomClaimTypes.BranchOfficeId,
        application.BranchOfficeId,
        OpenIdConnectConstants.Destinations.IdentityToken, OpenIdConnectConstants.Destinations.AccessToken);
      }

      // a 'branch auditor' workplace application
      // doesn't have a 'department id' claim
      if (!string.IsNullOrEmpty(application.DepartmentId))
      {
        identity.AddClaim(
          CustomClaimTypes.DepartmentId,
          application.DepartmentId,
          OpenIdConnectConstants.Destinations.IdentityToken, OpenIdConnectConstants.Destinations.AccessToken);
      }
      return ticket;
    }

    private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, ApplicationUser user)
    {
      // Create a new ClaimsPrincipal containing the claims that
      // will be used to create an id_token, a token or a code.
      var principal = await _signInManager.CreateUserPrincipalAsync(user);

      principal.AddUserTenantAndOrgStructureClaims(user.TenantId, user.BranchOfficeId, user.DepartmentId);

      // Create a new authentication ticket holding the user identity.
      var ticket = new AuthenticationTicket(
        principal, new AuthenticationProperties(),
        OpenIdConnectServerDefaults.AuthenticationScheme);

      //if (!request.IsRefreshTokenGrantType())
      //{
      // Set the list of scopes granted to the client application.
      // Note: the offline_access scope must be granted
      // to allow OpenIddict to return a refresh token.
      ticket.SetScopes(new[]
      {
        OpenIdConnectConstants.Scopes.OpenId,
        OpenIdConnectConstants.Scopes.Email,
        OpenIdConnectConstants.Scopes.Phone,
        OpenIdConnectConstants.Scopes.Profile,
        OpenIdConnectConstants.Scopes.OfflineAccess,
        "roles"
      }.Intersect(request.GetScopes()));
      //}

      ticket.SetResources(request.Resources);

      // Note: by default, claims are NOT automatically included in the access and identity tokens.
      // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
      // whether they should be included in access tokens, in identity tokens or in both.

      foreach (var claim in ticket.Principal.Claims)
      {
        // Never include the security stamp in the access and identity tokens, as it's a secret value.
        if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
        {
          continue;
        }

        var destinations = new List<string> { OpenIdConnectConstants.Destinations.AccessToken };

        // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
        // The other claims will only be added to the access_token, which is encrypted when using the default format.
        if ((claim.Type == OpenIdConnectConstants.Claims.Subject && ticket.HasScope(OpenIdConnectConstants.Scopes.OpenId)) ||
            (claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
            (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope("roles")) ||
            (claim.Type == CustomClaimTypes.Permission && ticket.HasScope("roles")))
        {
          destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
        }

        claim.SetDestinations(destinations);
      }

      var identity = principal.Identity as ClaimsIdentity;

      identity.AddClaim(
        CustomClaimTypes.TenantId,
        user.TenantId.ToString(),
        OpenIdConnectConstants.Destinations.IdentityToken);

      if (ticket.HasScope(OpenIdConnectConstants.Scopes.Email))
      {
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
          identity.AddClaim(
            CustomClaimTypes.Email,
            user.Email,
            OpenIdConnectConstants.Destinations.IdentityToken);
        }
      }

      return ticket;
    }
  }
}

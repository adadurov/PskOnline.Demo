namespace PskOnline.Server.Service.Helpers
{
  using PskOnline.Server.Authority.API.Constants;
  using PskOnline.Server.Service.ViewModels;
  using System;
  using System.Linq;

  public static class WorkplaceScopesValidator
  {
    public static Tuple<bool, string> CheckRequestedDepartmentWorkplaceScopes(WorkplaceCredentialsRequestDto request)
    {
      return ValidateScopes(request, new[] { PskOnlineScopes.DeptAuditorWorkplace, PskOnlineScopes.DeptOperatorWorkplace });
    }

    public static Tuple<bool, string> CheckRequestedBranchWorkplaceScopes(WorkplaceCredentialsRequestDto request)
    {
      return ValidateScopes(request, new[] { PskOnlineScopes.BranchAuditorWorkplace });
    }

    private static Tuple<bool, string> ValidateScopes(WorkplaceCredentialsRequestDto request, string[] validScopes)
    {
      var requestedScopes = request.Scopes.Split(' ');
      if (requestedScopes.Any(s => ! validScopes.Contains(s)))
      {
        var validString = string.Join(", ", validScopes);
        return Tuple.Create(false, $"Invalid scope requested. Valid scopes are: {validString}");
      }
      return Tuple.Create(true, default(string));
    }
  }
}

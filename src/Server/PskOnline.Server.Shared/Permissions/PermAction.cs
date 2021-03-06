﻿namespace PskOnline.Server.Shared.Permissions
{
  /// <summary>
  /// Defines actions, that are in turn used to define permissions
  /// </summary>
  public static class PermAction
  {
    public const string Manage = nameof(Manage);

    public const string Submit = nameof(Submit);

    public const string AssignRole = nameof(AssignRole);

    public const string View = nameof(View);
  }
}

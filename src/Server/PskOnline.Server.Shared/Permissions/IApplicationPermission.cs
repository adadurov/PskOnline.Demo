namespace PskOnline.Server.Shared.Permissions
{
  public interface IApplicationPermission
  {
    string Name { get; }

    /// <summary>
    /// Dot-separated Type.Scope.Action
    /// This attribute is persisted in backing store (e.g. for roles)
    /// </summary>
    /// <remarks>
    /// This property is related to Type, Scope & Action 
    /// properties and must not be modified separately
    /// </remarks>
    string Value { get; }

    string GroupName { get; }

    string Description { get; }

    /// <summary>
    /// </summary>
    /// <remarks>
    /// This property is related to 'value',
    /// and must not be modified separately
    /// </remarks>
    string Type { get; }

    /// <summary>
    /// </summary>
    /// <remarks>
    /// This property is related to 'value',
    /// and must not be modified separately
    /// </remarks>
    string Scope { get; }

    /// <summary>
    /// </summary>
    /// <remarks>
    /// This property is related to 'value',
    /// and must not be modified separately
    /// </remarks>
    string Action { get; }
  }
}

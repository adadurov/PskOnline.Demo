namespace PskOnline.Server.Shared.Exceptions
{
  using System;

  public class ItemNotFoundException : Exception
  {
    public ItemNotFoundException(string id, string entityTypeName)
      : base($"The {entityTypeName} with Id={id} was not found")
    {
    }

    internal ItemNotFoundException(string keyName, string keyValue, string entityTypeName)
      : base($"The {entityTypeName} with {keyName}={keyValue} was not found")
    {
    }

    public static ItemNotFoundException NotFoundByKey(string keyName, string keyValue, string entityTypeName)
    {
      return new ItemNotFoundException(keyName, keyValue, entityTypeName);
    }
  }
}

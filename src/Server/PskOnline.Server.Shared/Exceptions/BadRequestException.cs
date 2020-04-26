namespace PskOnline.Server.Shared.Exceptions
{
  using System;

  public class BadRequestException : Exception
  {
    public BadRequestException(string message)
      : base(message)
    {
    }

    private BadRequestException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public static BadRequestException BadReference(string message)
    {
      return new BadRequestException(message);
    }

    public static BadRequestException BadReference(string message, Exception innerException)
    {
      return new BadRequestException(message, innerException);
    }
    /// <summary>
    /// Ultimately used to convert to 'BadRequest'
    /// when client tries to create an entity with a duplicate name
    /// or update an entity changing its name to the value 
    /// that is already taken by another entity
    /// </summary>
    /// <param name="entityTypeName"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static BadRequestException NamedEntityExists(string entityTypeName, string name)
    {
      if (!string.IsNullOrEmpty(entityTypeName))
      {
        if (entityTypeName.StartsWith("e")) // FIXME -- what the heck is this for?
        {
          return new BadRequestException(
            $"An {entityTypeName} with the same name ({name}) already exists.");
        }
        return new BadRequestException(
          $"A {entityTypeName} with the same name ({name}) already exists.");
      }
      return new BadRequestException(
        $"An entity with the same name ({name}) already exists.");
    }

  }
}

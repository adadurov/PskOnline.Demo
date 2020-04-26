namespace PskOnline.Server.Shared.Exceptions
{
  using System;

  public class ConflictException : Exception
  {
    /// <summary>
    /// Ultimately used to indicate an extremely rare case when client tries to create (post)
    /// a new entity with an ID already specified, but the server found that the ID
    /// is already in use
    /// </summary>
    /// <param name="entityTypeName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static ConflictException IdConflict(string entityTypeName, Guid id)
    {
      var ex = new ConflictException(
        $"The {entityTypeName} with id={id} already exists");
      return ex;
    }

    private ConflictException(string message) : base(message)
    {
    }

  }
}

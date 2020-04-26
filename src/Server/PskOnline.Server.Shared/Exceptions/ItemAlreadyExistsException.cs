namespace PskOnline.Server.Shared.Exceptions
{
  using System;

  public class ItemAlreadyExistsException : Exception
  {
    /// <summary>
    /// Client posts a new resource and server recognizes is to be the same
    /// as an already existing resource.
    /// Intended use is to ensure proper failover and retry logic in the use cases that don't involve human operator.
    /// For example: retransmission of Inspection entites and retransmission of test entities,
    /// that are synchronized automatically.
    /// </summary>
    /// <param name="entityTypeName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static ItemAlreadyExistsException MatchingEntityExists(string entityTypeName, Guid id)
    {
      var ex = new ItemAlreadyExistsException(
        $"The {entityTypeName} with id={id} already exists");
      ex.Id = id;
      return ex;
    }

    public Guid? Id { get; set; }

    private ItemAlreadyExistsException(string message)
      : base(message)
    {
    }
  }
}

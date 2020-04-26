namespace PskOnline.Client.Api
{
  using System;

  public class ConflictException : Exception
  {
    public ConflictException(string message)
      : base(message)
    {
    }

    private ConflictException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public static ConflictException HttpConflict(ApiErrorDto dto)
    {
      return new ConflictException(dto.Error);
    }
  }
}

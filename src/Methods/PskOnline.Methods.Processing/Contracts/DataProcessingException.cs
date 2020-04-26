using System;

namespace PskOnline.Methods.Processing.Contracts
{
  /// <summary>
  /// Исключение, возникшее в процессе обработки данных
  /// </summary>
  public class DataProcessingException : Exception
  {
    public DataProcessingException(string message)
      : base(message)
    {
    }

  }
}

using System;

namespace PskOnline.Methods.Processing.Contracts
{
  /// <summary>
  /// ����������, ��������� � �������� ��������� ������
  /// </summary>
  public class DataProcessingException : Exception
  {
    public DataProcessingException(string message)
      : base(message)
    {
    }

  }
}

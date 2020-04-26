namespace PskOnline.Methods.Processing.Contracts
{
  using System;

  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.Settings;

  /// <summary>
	/// ���������, ����� ������� ������� ���������� � ��������� ���������� ����������� ������������.
	/// </summary>
	public interface IMethodDataProcessor : IMethodCustomizable, IDisposable
	{
    /// <summary>
    /// ���������� ���������� ������ ����������� ������ �����.
    /// ���������� ��� �������� ������������ � �������������� ���������� 
    /// ����������� � �� ����������� ��������� ������ �����.
    /// </summary>
	  int GetProcessorVersion();

    /// <summary>
    /// ������������ ������.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>��� �������� ��������� ���������� ��������� ������.</returns>
    /// <exception cref="DataProcessingException">��������� �� ������� �� �����-���� �������.</exception>
    IMethodProcessedData ProcessData(IMethodRawData data);

	}
}

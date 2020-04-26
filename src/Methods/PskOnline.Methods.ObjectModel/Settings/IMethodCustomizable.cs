namespace PskOnline.Methods.ObjectModel.Settings
{
  using PskOnline.Methods.ObjectModel.Settings;

  /// <summary>
  /// ���������� ��������� ��� ������������� �������, ������� ����� ��������� ���������.
  /// </summary>
  public interface IMethodCustomizable
	{
		/// <summary>
		/// ���������� ����� �������, ��������� ������� ������������.
		/// </summary>
		/// <returns>������, ���������� ���������.</returns>
		IMethodSettings Get();

    /// <summary>
    /// ������������� ��������� �� ����������� �������.
    /// ����������� ����������, ���� ������� ������ ������������ ���������.
    /// ���� ����� �������� ��������� ��� ��� ��� ������...
    /// ��������, �������� �������������� �������������� ������������ ����������.
    /// </summary>
    void Set(IMethodSettings settings);

    /// <summary>
    /// �������� ������ �������� ������ ���������, � ���,
    /// ����� ���������� ������ ��� ������ ��� ����
    /// �� ������� ��������� ������ ���������.
    /// </summary>
    /// <param name="pack"></param>
    void Set(IMethodSettingsPack pack);
    
		/// <summary>
		/// ���������� ��������� �������������� �������.
		/// </summary>
		Category GetCategory();
	}

}

namespace PskOnline.Methods.ObjectModel.Settings
{
  /// <summary>
  /// ������������ ����� ���� �� ����������� (����������) ��� ����������
  /// ������������ ������������ ����������. ���� ���� ����� �������� �
  /// ����������� ������� ��������������, ���, ��������, ���,
  /// ������� ����� ���� ���������� �� ���� ������� ���, ������� ��� ���
  /// �������, ������� ������ ��� ������� ������-���������, ��������,
  /// Polar H7, CorSense � �.�.
  /// </summary>
  public class DeviceConfig
  {
    public DeviceConfig()
    {
      SignalTypes = null;
      DeviceFunctions = null;
    }
    /// <summary>
    /// ��������� ���� ������� ����������
    /// (���� ������������ � ���������� ��������)
    /// ��������: ���, ���
    /// </summary>
    public string[] SignalTypes { get; set; }

    /// <summary>
    /// ��������� ������� ����������
    /// (����, �������, ������ � �.�.)
    /// </summary>
    public string[] DeviceFunctions { get; set; }

  }
}
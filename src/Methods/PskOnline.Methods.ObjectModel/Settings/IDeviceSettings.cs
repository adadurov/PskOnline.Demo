namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;

  /// <summary>
  /// ��������� ���������� ��� ���������� ������������.
  /// </summary>
  public interface IDeviceSettings : ICloneable
  {
    /// <summary>
    /// ����� �� ������ �������� ������������ ����������?
    /// </summary>
    bool IsDeviceNeeded { get; }

    /// <summary>
    /// 
    /// </summary>
    DeviceConfig[] AcceptableConfigs { get; set; }
  }
}
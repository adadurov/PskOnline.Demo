namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;

  /// <summary>
  /// Настройки устройства для проведения обследования.
  /// </summary>
  public interface IDeviceSettings : ICloneable
  {
    /// <summary>
    /// Нужно ли вообще пытаться использовать устройство?
    /// </summary>
    bool IsDeviceNeeded { get; }

    /// <summary>
    /// 
    /// </summary>
    DeviceConfig[] AcceptableConfigs { get; set; }
  }
}
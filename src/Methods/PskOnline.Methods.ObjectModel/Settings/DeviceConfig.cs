namespace PskOnline.Methods.ObjectModel.Settings
{
  /// <summary>
  /// Представляет собой ОДНУ из необходимых (приемлемых) для проведения
  /// обследования конфигураций устройства. Если тест готов работать с
  /// несколькими разными конфигурациями, как, например, ВКР,
  /// который может быть реализован на базе датчика ФПГ, датчика ЭКГ или
  /// датчика, который выдает уже готовые кардио-интервалы, например,
  /// Polar H7, CorSense и т.п.
  /// </summary>
  public class DeviceConfig
  {
    public DeviceConfig()
    {
      SignalTypes = null;
      DeviceFunctions = null;
    }
    /// <summary>
    /// Требуемые типы каналов устройства
    /// (типы подключенных к устройству датчиков)
    /// Например: ФПГ, КГР
    /// </summary>
    public string[] SignalTypes { get; set; }

    /// <summary>
    /// Требуемые функции устройства
    /// (ПЗМР, тэппинг, тремор и т.п.)
    /// </summary>
    public string[] DeviceFunctions { get; set; }

  }
}
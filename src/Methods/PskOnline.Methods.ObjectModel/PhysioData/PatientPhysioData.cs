namespace PskOnline.Methods.ObjectModel.PhysioData
{
  using System.Collections.Generic;

  /// <summary>
  /// Класс для хранения данных одного физиологического канала,
  /// записанных с одного обследуемого.
  /// </summary>
  /// <remarks>
  /// Этот класс является частью модели, которая сохраняется в репозитории.
  /// Переименовывание полей или изменение их типов
  /// скорее всего будет являться "breaking change", требующим дополнительной обработки
  /// для сохранения обратной совместимости.
  /// </remarks>
  public class PatientPhysioData
  {
    public string PatientId { get; set; }

    /// <summary>
    /// Все данные всех каналов
    /// channel_id:string -> channelData:ChannelData
    /// </summary>
    public Dictionary<string, ChannelData> Channels { get; set; }

    /// <summary>
    /// Установленные маркеры
    /// markerId:long -> marker:Marker
    /// </summary>
    public Dictionary<long, Marker> Markers { get; set; }

    public int MaxMarkerId { get; set; }

    /// <summary>
    /// Содержит сразу все физиологические данные,
    /// записанные с помощью одного _логического_ устройства
    /// (которое может использовать для получения данных несколько физических устройств).
    /// </summary>
    public PatientPhysioData()
    {
    }

  }
}
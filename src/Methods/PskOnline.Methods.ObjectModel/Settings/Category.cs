namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;

	/// <summary>
  /// Перечисление имеющихся категорий настроек для методик.
  /// Значения должны быть битовыми флагами.
  /// </summary>
  [Flags]
	public enum Category : int
	{
    /// <summary>
    /// Неизвестно
    /// </summary>
		None = 0x0,

    /// <summary>
    /// "Обследование"
    /// </summary>
    Test = 0x1,

    /// <summary>
    /// "Устройство" -- настройки устройства
    /// </summary>
    Device = 0x2,

    /// <summary>
    /// "Дополнительная нагрузка"
    /// </summary>
    ExtraLoad = 0x4,

    /// <summary>
    /// "Обработка"
    /// </summary>
    Processing = 0x8,

    /// <summary>
    /// "Представление" (экранное представление данных)
    /// </summary>
    Presentation = 0x10,

    /// <summary>
    /// "Представление" (экранное представление данных)
    /// </summary>
    Report = 0x20
	}

	/// <summary>
	/// Служебный класс для преобразования категории в строку,
	/// которая может использоваться в UI.
	/// FIXME: move the converter to a more appropriate place
	/// </summary>
	public static class CategoryConverter
	{
		public static string ToString(Category cat)
		{
      return category.ResourceManager.GetString(cat.ToString());
		}
	}

}
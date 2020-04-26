namespace PskOnline.Methods.ObjectModel.Settings
{
  using PskOnline.Methods.ObjectModel.Settings;

  /// <summary>
  /// Определяет интерфейс для методического объекта, который может принимать настройки.
  /// </summary>
  public interface IMethodCustomizable
	{
		/// <summary>
		/// Возвращает копию объекта, хранящего текущую конфигурацию.
		/// </summary>
		/// <returns>Объект, содержащий настройки.</returns>
		IMethodSettings Get();

    /// <summary>
    /// Устанавливает настройки из переданного объекта.
    /// Выбрасывает исключение, если получен объект неподходящей категории.
    /// Тест может изменить параметры так как ему удобно...
    /// Например, добавить альтернативную поддерживаемую конфигурацию устройства.
    /// </summary>
    void Set(IMethodSettings settings);

    /// <summary>
    /// Передает массив настроек разных категорий, с тем,
    /// чтобы вызываемый объект сам выбрал для себя
    /// из массива настройки нужной категории.
    /// </summary>
    /// <param name="pack"></param>
    void Set(IMethodSettingsPack pack);
    
		/// <summary>
		/// Возвращает категорию настраиваемого объекта.
		/// </summary>
		Category GetCategory();
	}

}

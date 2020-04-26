namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;
  using System.Collections.Generic;

	/// <summary>
  /// Коллекция-контейнер настроек для методики.
  /// Содержит настройки всех категорий:
  /// для проведения обследования, для обработки данных,
  /// для отображения результатов на экране, для генерации отчета.
  /// В будущем могут быть добавлены другие категории.
  /// 
  /// Используется для сохранения настроек, для чтения и записи
  /// </summary>
  public interface IMethodSettingsPack : ICloneable, IEnumerable<IMethodSettings>
	{
		/// <summary>
		/// Добавляет настройки в пакет.
		/// </summary>
		/// <param name="settings">Настройки, которые должны быть добавлены.</param>
		void Add(IMethodSettings settings);

    /// <summary>
    /// Удаляет настройки указанной категории.
    /// </summary>
    /// <param name="category"></param>
    void Remove(Category category);

		/// <summary>
		/// Индексатор для получения/изменения настроек нужной категории.
		/// </summary>
		IMethodSettings this[Category category] {get;set;}

    /// <summary>
    /// Количество элементов настройки
    /// </summary>
    int Count { get; }
	}
}

namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;

	/// <summary>
  /// Интерфейс объекта, хранящего настройки различных аспектов методик, подлежащих настройке пользователем.
  /// Предоставляет доступ к интерфейсу пользователя для редактирования содержащихся настроек
  /// (создает элементы управления, используемые для редактирования содержимого).
  /// 
  /// TODO: BUGBUG: Check if the statement below is actually true.
  /// Для сравнения значений настроек используется функция
  /// bool System.Object.Equals(System.Object obj).
  /// Для сравнения идентичности используется оператор ==
  /// </summary>  
  public interface IMethodSettings : ICloneable
	{
		/// <summary>
		/// Возвращает категорию настроек.
		/// </summary>
		Category GetCategory();

		/// <summary>
		/// Возвращает методику, к которой относятся настройки.
		/// </summary>
		/// <returns></returns>
		string GetMethodId();

    /// <summary>
    /// Устанавливает настройки по умолчанию.
    /// </summary>
    void Default();

    /// <summary>
    /// Копирует настройки из объекта src
    /// </summary>
    /// <param name="source"></param>
    void CopyFrom(IMethodSettings source);

	}
}

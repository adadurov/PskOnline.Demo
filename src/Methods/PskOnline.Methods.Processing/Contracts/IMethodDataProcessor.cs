namespace PskOnline.Methods.Processing.Contracts
{
  using System;

  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.Settings;

  /// <summary>
	/// Интерфейс, через который система инициирует и управляет обработкой результатов обследования.
	/// </summary>
	public interface IMethodDataProcessor : IMethodCustomizable, IDisposable
	{
    /// <summary>
    /// Возвращает работающую версию обработчика данных теста.
    /// Необходимо для контроля актуальности и своевременного обновления 
    /// сохраненных в БД результатов обработки данных теста.
    /// </summary>
	  int GetProcessorVersion();

    /// <summary>
    /// Обрабатывает данные.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>При успешной обработке возвращает выхоодные данные.</returns>
    /// <exception cref="DataProcessingException">Обработка не удалась по какой-либо причине.</exception>
    IMethodProcessedData ProcessData(IMethodRawData data);

	}
}

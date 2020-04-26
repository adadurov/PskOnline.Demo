namespace PskOnline.Methods.Processing.Logic
{
  using System;

  using PskOnline.Methods.ObjectModel;
  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.PhysioData;
  using PskOnline.Methods.ObjectModel.Settings;
  using PskOnline.Methods.Processing.Contracts;

  /// <summary>
	/// 
	/// </summary>
	public abstract class BasicDataProcessor : IMethodDataProcessor
	{

	  public virtual void Dispose() 
    {
    }

    /// <summary>
    /// Это результаты обработки данных по методике.
    /// 
    /// Это поле используется для поддержки будущих комплексных обработчиков на основе сценариев,
    /// (сбор статистики, анализ показателей групп, анализ динамики показателей и т.п.).
    /// 
    /// Возвращается оболочке через свойство ProcessorOutputData,
    /// унаследованное от интерфейса IMethodDataProcessor.
    /// 
    /// Каждый конкретный DataProcessor должен создать объект класса-наследника 
    /// BasicOutData и заполнить его результатами первичной обработки,
    /// (в соответствие с ТЗ на конкретную методику).
    /// 
    /// Классы-наследники в своих конструкторах инициализируют значение
    /// этого свойства новым экземпляр класса-наследника BasicOutData
    /// с дополнительными полями (пример см. в реализации методики ПЗМР).
    /// Затем данный класс (BasicDataProcessor) заполняет этот объект своими данными
    /// (одинаковыми для всех методик: ФИО обследуемых, методика, дата и время
    /// начала и окончания теста, сигналы и прочее, а обработчик конкретной методики --
    /// своими специализированными результатами.
    /// 
    /// </summary>
    protected IMethodProcessedData ProcessorOutputData { get ; set; }

    #region IMethodDataProcessor Members

	  public abstract int GetProcessorVersion();

    public virtual IMethodProcessedData ProcessData(IMethodRawData source_data)
		{
      if( ProcessorOutputData == null )
      {
        throw new InvalidOperationException(
          "поле m_ProcessorOutputData должно быть инициализировано в конструкторе класса-наследника!!!");
      }

			// Здесь просто пробрасывается на выход информация об обследовании
			// 1. Наименование методики обследования
			// 2. Обследуемый
			// 3. Дата и время проведения обследования
      // 4. Тип обследования (тренировка, предсменный контроль и т.д.)

      var ti = source_data.TestInfo;

      if( ti != null )
      {
        ProcessorOutputData.TestInfo = ti;
      }

	    return ProcessorOutputData;
		}

		#endregion

		#region ICustomizable Members
		public Category GetCategory()
		{
			return Category.Processing;
		}

		public abstract IMethodSettings Get();

		public abstract void Set(IMethodSettings settings);

    public void Set(IMethodSettingsPack pack)
    {
      Set(pack[GetCategory()]);
    }

    #endregion

    private log4net.ILog log = log4net.LogManager.GetLogger(typeof(BasicDataProcessor));
  }
}

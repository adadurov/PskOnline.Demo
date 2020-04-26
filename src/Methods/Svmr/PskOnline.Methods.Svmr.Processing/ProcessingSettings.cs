namespace PskOnline.Methods.Svmr.Processing
{
  using PskOnline.Methods.ObjectModel.Settings;
  using PskOnline.Methods.Processing.Settings;
  using PskOnline.Methods.Svmr.ObjectModel;

  /// <summary>
  /// Хранит настройки обработки результатов для методики ПЗМР.
  /// </summary>
  public class ProcessingSettings : BasicProcessingSettings
	{
		/// <summary>
    /// Порог ложной реакции.
    /// Все более быстрые реакции будут считаться "ложными".
		/// </summary>
		public float MinReactionTimeSeconds;

		/// <summary>
    /// Порог запаздывающей реакции.
    /// Все более медленные реакции будут считаться "запаздывающими".
		/// </summary>
		public float MaxReactionTimeSeconds;

		/// <summary>
		/// Поправка на задержки (механика клавиатуры/мыши и частота кадров на мониторе)
    /// Секунды, формат с плавающей точкой одинарной точности
		/// </summary>
		public float CorrectionTimeSeconds;
		
		public ProcessingSettings() : base(SvmrMethodId.MethodId )
		{
			PrivateDefault();
		}

		public override void Default()
		{
      base.Default();
		  PrivateDefault();
		}

	  private void PrivateDefault()
	  {
	    MinReactionTimeSeconds = 0.15f;
	    MaxReactionTimeSeconds = 1.00f;
	    CorrectionTimeSeconds = 0.050f;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode ();
    }

    /// <summary>
    /// Реализует сравнение ЗНАЧЕНИЙ объектов, а не ссылок.
    /// </summary>
    public override bool Equals(object obj)
    {
      if( obj is ProcessingSettings other )
      {
        return (MaxReactionTimeSeconds == other.MaxReactionTimeSeconds) &&
               (MinReactionTimeSeconds == other.MinReactionTimeSeconds) &&
               (CorrectionTimeSeconds == other.CorrectionTimeSeconds);
      }
      return false;
    }

    /// <summary>
    /// Копирует настройки из объекта source
    /// </summary>
    /// <param name="source"></param>
    public override void CopyFrom(IMethodSettings source)
    {
      base.CopyFrom(source);
      if( source is ProcessingSettings src )
      {
        MinReactionTimeSeconds = src.MinReactionTimeSeconds;
        MaxReactionTimeSeconds = src.MaxReactionTimeSeconds;
        CorrectionTimeSeconds = src.CorrectionTimeSeconds;
      }
    }

    public override object Clone()
    {
      ProcessingSettings o = new ProcessingSettings();
      o.CopyFrom(this);
      return o;
    }

	}
}

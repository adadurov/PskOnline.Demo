namespace PskOnline.Methods.Hrv.Processing.Settings
{
  using System;
  using System.Xml.Serialization;

  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Hrv.Processing.Logic;

  using PskOnline.Methods.ObjectModel.Settings;
  using PskOnline.Methods.Processing.Settings;

  /// <summary>
  /// Хранит настройки представления результатов для методики ВКР.
  /// </summary>
  public class ProcessingSettings : BasicProcessingSettings
	{
    /// <summary>
    /// Требуется ли отбраковка по качеству фрагментов сигнала
    /// FIXME: в будущем будет всегда true, добавлена временно,
    /// с первой упрощенной реализацией для условий Русгидро,
    /// чтобы не сломались тесты в других условиях и на других сигналах.
    /// Точнее, чтобы не забыть поменять тесты на других сигналах,
    /// когда сделаем нормальную реализацию.
    /// </summary>
    [NonSerialized]
    public bool RejectLowQualitySignalAreas;

    /// <summary>
    /// Разрешена ли отбраковка по максимальной и минимальной длительности кардионитервала.
    /// </summary>
    [XmlElement("Отбраковка_по_длительности")]
    public bool RejectUsingMinMaxNNTime;
      
    /// <summary>
    /// Минимально разрешенная длительность NN-интервала, мс
    /// Все более короткие интервалы будет считаться артефактами и в статистику не войдут.
    /// </summary>
    [XmlElement("Мин_длительноcть_кардиоинтервала_мс")]
    public float MinIntervalMilliseconds;

    /// <summary>
    /// Введенная пользователем максимально разрешенная длительность NN-интервала, мс
    /// Все более длинные интервалы будет считаться артефактами и в статистику не войдут.
    /// </summary>
    [XmlElement("Макс_длительноcть_кардиоинтервала_мс")]
    public float MaxIntervalMilliseconds;

    /// <summary>
    /// Отбраковка по относительному изменению последовательных кардиоинтервалов.
    /// </summary>
    [XmlElement("Отбраковка_по_относительному_изменению")]
    public bool RejectUsingRelativeNNDelta;

    /// <summary>
    /// Максимально разрешенное относительное изменение следующего интервала, %%
    /// </summary>
    [XmlElement("Максимальное_относительное_изменение_следующего_интервала")]
    public float MaxIntervalDeltaRelative;

	  /// <summary>
	  /// Персональная норма
	  /// </summary>
    [XmlIgnore]
    public PersonalHrvNorm PersonalNorm { get; set; }

	  /// <summary>
	  /// Минимально разрешенная длительность NN-интервала с учетом 
	  /// значения поля bRejectUsingMinMaxNNTime
	  /// </summary>
	  public double GetApplicableMinIntervalLength()
	  {
     return RejectUsingMinMaxNNTime ? MinIntervalMilliseconds : 0;
	  }

	  /// <summary>
	  /// Максимально разрешенная длительность NN-интервала с учетом 
	  /// значения поля bRejectUsingMinMaxNNTime
	  /// </summary>
	  public double GetApplicableMaxIntervalLength()
	  {
     return this.RejectUsingMinMaxNNTime ? this.MaxIntervalMilliseconds : double.PositiveInfinity;
	  }


    public ProcessingSettings()
	    : base(PskOnline.Methods.Hrv.ObjectModel.HrvMethodId.MethodId)
	  {
		  Default();
	  }

	  /// <summary>
	  /// Если не применяется отбраковка по максимальному изменению, возвращает Positive Infinity
	  /// </summary>
	  public double GetApplicableMaxIntervalDeltaRelative()
	  {
	    return this.RejectUsingRelativeNNDelta ? this.MaxIntervalDeltaRelative : double.PositiveInfinity;
	  }

    public override void Default()
    {
      base.Default();

      RejectLowQualitySignalAreas = true;
      this.RejectUsingMinMaxNNTime = true;
      this.MinIntervalMilliseconds = (float)GlobalMethodLimits.MinCardioCycleInMilliseconds;
      this.MaxIntervalMilliseconds = (float)GlobalMethodLimits.MaxCardioCycleInMilliseconds;
      this.RejectUsingRelativeNNDelta = true;
      this.MaxIntervalDeltaRelative = (float)(GlobalMethodLimits.MaxRelativeDifferenceOfSuccessiveIntervals * 100); // в %%!!!
      this.PersonalNorm = PersonalHrvNorm.GetInstance(DefaultStateMatrix.GetHeartRateMidPointForSettings());
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
        RejectLowQualitySignalAreas = src.RejectLowQualitySignalAreas;
        this.RejectUsingMinMaxNNTime = src.RejectUsingMinMaxNNTime;
        this.MinIntervalMilliseconds = src.MinIntervalMilliseconds;
        this.MaxIntervalMilliseconds = src.MaxIntervalMilliseconds;
        this.RejectUsingRelativeNNDelta = src.RejectUsingRelativeNNDelta;
        this.MaxIntervalDeltaRelative = src.MaxIntervalDeltaRelative;
        this.PersonalNorm = new PersonalHrvNorm(src.PersonalNorm);
      }
    }

    public override object Clone()
    {
      ProcessingSettings o = new ProcessingSettings();
      o.CopyFrom(this);
      return o;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj is ProcessingSettings temp)
      {
        return (temp.RejectUsingMinMaxNNTime == RejectUsingMinMaxNNTime) &&
               (temp.MaxIntervalMilliseconds == MaxIntervalMilliseconds) &&
               (temp.MinIntervalMilliseconds == MinIntervalMilliseconds) &&
               (temp.RejectUsingRelativeNNDelta == RejectUsingRelativeNNDelta) &&
               (temp.MaxIntervalDeltaRelative == MaxIntervalDeltaRelative) &&
               (temp.m_category == m_category) &&
               (temp.m_method_id == m_method_id) &&
               (temp.PersonalNorm == PersonalNorm);
      }
      return false;
    }

  }

}

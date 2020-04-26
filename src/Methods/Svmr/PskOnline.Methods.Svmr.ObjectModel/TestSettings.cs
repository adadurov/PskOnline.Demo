namespace PskOnline.Methods.Svmr.ObjectModel
{
  using System;
  using System.Xml.Serialization;
  using System.Drawing;
  using PskOnline.Methods.ObjectModel.Settings;

  /// <summary>
  /// Этот класс содержит спецификацию требуемого
  /// процесса обследования по методике ПЗМР.
  /// </summary>
  public class TestSettings : BasicSettings
  {
		/// <summary>
		/// Количество зачетных стимулов.
		/// </summary>
		[XmlElement("Количество_зачетных_стимулов")]
		public int AccountedStimulCount;

    /// <summary>
    /// Количество "тренировочных" стимулов.
    /// </summary>
    [XmlElement("Количество_тренировочных_стимулов")]
    public int TrainingStimulCount;

    /// <summary>
    /// Следует ли показывать информационное окно 
    /// после окончания "тренировочных" стимулов.
    /// </summary>
    [XmlElement("Информировать_об_окончании_тренировки")]
    public bool ShowEndTrainingPrompt;

    /// <summary>
		/// Минимальное время между стимулами, с.
		/// </summary>
		[XmlElement("Минимальное_время_между_стимулами")]
		public double StimulMinTime;

		/// <summary>
		/// Максимальное время между стимулами, с.
		/// </summary>
		[XmlElement("Максимальное_время_между_стимулами")]
		public double StimulMaxTime;

		/// <summary>
		/// Время горения светодиода
		/// (до засчитывания пропуска стимула).
		/// </summary>
		[XmlElement("Таймаут")]
		public double StimulOnTime;

		/// <summary>
		/// Форма стимула.
		/// </summary>
		[XmlElement("Форма_стимула")]
		public StimulusShape StimulShape;

		/// <summary>
		/// Размер (диаметр) стимула (задается в сантиметрах).
		/// </summary>
		[XmlElement("Размер_стимула")]
		public float StimulSize;

		[XmlElement("Использование_клавиши_готовности")]
		public bool UseReadyKey;

		[XmlElement("Клавиша_готовности")]
		public ActionKey ReadyKey;

		[XmlElement("Клавиша_реакции")]
		public ActionKey ReactionKey;

    /// <summary>
    /// Цвет стимула
    /// Чтобы получить его в виде Color
    /// используйте StimulusColor
    /// </summary>
    [XmlElement("Цвет_стимула")]
    public System.Drawing.Color StimulusColor;

    /// <summary>
    /// Цвет фона.
    /// Чтобы получить его в виде Color
    /// используйте BackgroundColor
    /// </summary>
    [XmlElement("Цвет_фона")]
    public System.Drawing.Color BackgroundColor;

    /// <summary>
    /// Максимальный коэффициент ошибок, проценты (0..100).
    /// При превышении будем ругаться.
    /// </summary>
    [XmlElement("Порог_ошибок")]
    public double RuntimeErrorThreshold;

		[XmlElement("Реализация")]
		public ImplementationType Implementation;

		/// <summary>
		/// Конструктор.
		/// </summary>
		public TestSettings() : base(Category.Test, SvmrMethodId.MethodId)
		{
			PrivateDefault();
		}
		
    /// <summary>
    /// Копирует настройки из объекта source
    /// </summary>
    /// <param name="source"></param>
    public override void CopyFrom(IMethodSettings source)
    {
      base.CopyFrom(source);
      if( source is TestSettings src )
      {
        AccountedStimulCount = src.AccountedStimulCount;
        TrainingStimulCount = src.TrainingStimulCount;
        StimulSize = src.StimulSize;
        StimulMinTime = src.StimulMinTime;
        StimulMaxTime = src.StimulMaxTime;
        StimulOnTime = src.StimulOnTime;
        StimulShape = src.StimulShape;
        Implementation = src.Implementation;
        StimulusColor = src.StimulusColor;
        BackgroundColor = src.BackgroundColor;
        RuntimeErrorThreshold = src.RuntimeErrorThreshold;
        UseReadyKey = src.UseReadyKey;
	      ReadyKey = src.ReadyKey;
	      ReactionKey = src.ReactionKey;
        ShowEndTrainingPrompt = src.ShowEndTrainingPrompt;
      }
    }

    public override object Clone()
    {
      TestSettings o = new TestSettings();
      o.CopyFrom(this);
      return o;
    }

	  /// <summary>
	  /// Устанавливает настройки по умолчанию.
	  /// </summary>
	  public override void Default()
	  {
	    PrivateDefault();
	  }

    private void PrivateDefault()
    { 
	    this.AccountedStimulCount = 100;
			this.TrainingStimulCount = 5;
			this.StimulSize = 5;
			this.StimulOnTime = 2;
			this.StimulMinTime = 2.0f;
			this.StimulMaxTime = 4.0f;
			this.StimulShape = StimulusShape.Circle;
			this.Implementation = ImplementationType.Software;
		  this.ShowEndTrainingPrompt = true;
      
			this.StimulusColor = Color.Green;
			this.BackgroundColor = Color.Black;

      this.UseReadyKey = false;
      this.ReadyKey = ActionKey.None;
      this.ReactionKey = ActionKey.Space;

      RuntimeErrorThreshold = 5;
    }

		#region Overriden Equals and GetHashCode members

		/// <summary>
		/// Реализует сравнение ЗНАЧЕНИЙ объектов, а не ссылок.
		/// </summary>
		public override bool Equals(object obj)
		{
		  if( obj is TestSettings other )
			{
				return  (this.AccountedStimulCount == other.AccountedStimulCount) &&
                (this.BackgroundColor == other.BackgroundColor) &&
                (this.RuntimeErrorThreshold == other.RuntimeErrorThreshold) &&
                (this.Implementation == other.Implementation) &&
                (this.ReadyKey == other.ReadyKey) &&
                (this.ReactionKey == other.ReactionKey) &&
                (this.StimulMaxTime == other.StimulMaxTime) &&
						    (this.StimulMinTime == other.StimulMinTime) &&
						    (this.StimulOnTime == other.StimulOnTime) &&
                (this.StimulShape == other.StimulShape) &&
                (this.StimulSize == other.StimulSize) &&
                (this.ShowEndTrainingPrompt == other.ShowEndTrainingPrompt) &&
                (this.TrainingStimulCount == other.TrainingStimulCount) &&
                (this.UseReadyKey == other.UseReadyKey) &&
                (this.m_category == other.m_category) &&
                (this.m_method_id == other.m_method_id);
			}
      return false;
		}
		
		/// <summary>
		/// Переопределено просто чтобы не ругался компилятор.
		/// </summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}

}
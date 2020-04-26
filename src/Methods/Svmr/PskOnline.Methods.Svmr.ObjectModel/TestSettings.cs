namespace PskOnline.Methods.Svmr.ObjectModel
{
  using System;
  using System.Xml.Serialization;
  using System.Drawing;
  using PskOnline.Methods.ObjectModel.Settings;

  /// <summary>
  /// ���� ����� �������� ������������ ����������
  /// �������� ������������ �� �������� ����.
  /// </summary>
  public class TestSettings : BasicSettings
  {
		/// <summary>
		/// ���������� �������� ��������.
		/// </summary>
		[XmlElement("����������_��������_��������")]
		public int AccountedStimulCount;

    /// <summary>
    /// ���������� "�������������" ��������.
    /// </summary>
    [XmlElement("����������_�������������_��������")]
    public int TrainingStimulCount;

    /// <summary>
    /// ������� �� ���������� �������������� ���� 
    /// ����� ��������� "�������������" ��������.
    /// </summary>
    [XmlElement("�������������_��_���������_����������")]
    public bool ShowEndTrainingPrompt;

    /// <summary>
		/// ����������� ����� ����� ���������, �.
		/// </summary>
		[XmlElement("�����������_�����_�����_���������")]
		public double StimulMinTime;

		/// <summary>
		/// ������������ ����� ����� ���������, �.
		/// </summary>
		[XmlElement("������������_�����_�����_���������")]
		public double StimulMaxTime;

		/// <summary>
		/// ����� ������� ����������
		/// (�� ������������ �������� �������).
		/// </summary>
		[XmlElement("�������")]
		public double StimulOnTime;

		/// <summary>
		/// ����� �������.
		/// </summary>
		[XmlElement("�����_�������")]
		public StimulusShape StimulShape;

		/// <summary>
		/// ������ (�������) ������� (�������� � �����������).
		/// </summary>
		[XmlElement("������_�������")]
		public float StimulSize;

		[XmlElement("�������������_�������_����������")]
		public bool UseReadyKey;

		[XmlElement("�������_����������")]
		public ActionKey ReadyKey;

		[XmlElement("�������_�������")]
		public ActionKey ReactionKey;

    /// <summary>
    /// ���� �������
    /// ����� �������� ��� � ���� Color
    /// ����������� StimulusColor
    /// </summary>
    [XmlElement("����_�������")]
    public System.Drawing.Color StimulusColor;

    /// <summary>
    /// ���� ����.
    /// ����� �������� ��� � ���� Color
    /// ����������� BackgroundColor
    /// </summary>
    [XmlElement("����_����")]
    public System.Drawing.Color BackgroundColor;

    /// <summary>
    /// ������������ ����������� ������, �������� (0..100).
    /// ��� ���������� ����� ��������.
    /// </summary>
    [XmlElement("�����_������")]
    public double RuntimeErrorThreshold;

		[XmlElement("����������")]
		public ImplementationType Implementation;

		/// <summary>
		/// �����������.
		/// </summary>
		public TestSettings() : base(Category.Test, SvmrMethodId.MethodId)
		{
			PrivateDefault();
		}
		
    /// <summary>
    /// �������� ��������� �� ������� source
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
	  /// ������������� ��������� �� ���������.
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
		/// ��������� ��������� �������� ��������, � �� ������.
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
		/// �������������� ������ ����� �� ������� ����������.
		/// </summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}

}
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
    /// ��� ���������� ��������� ������ �� ��������.
    /// 
    /// ��� ���� ������������ ��� ��������� ������� ����������� ������������ �� ������ ���������,
    /// (���� ����������, ������ ����������� �����, ������ �������� ����������� � �.�.).
    /// 
    /// ������������ �������� ����� �������� ProcessorOutputData,
    /// �������������� �� ���������� IMethodDataProcessor.
    /// 
    /// ������ ���������� DataProcessor ������ ������� ������ ������-���������� 
    /// BasicOutData � ��������� ��� ������������ ��������� ���������,
    /// (� ������������ � �� �� ���������� ��������).
    /// 
    /// ������-���������� � ����� ������������� �������������� ��������
    /// ����� �������� ����� ��������� ������-���������� BasicOutData
    /// � ��������������� ������ (������ ��. � ���������� �������� ����).
    /// ����� ������ ����� (BasicDataProcessor) ��������� ���� ������ ������ �������
    /// (����������� ��� ���� �������: ��� �����������, ��������, ���� � �����
    /// ������ � ��������� �����, ������� � ������, � ���������� ���������� �������� --
    /// ������ ������������������� ������������.
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
          "���� m_ProcessorOutputData ������ ���� ���������������� � ������������ ������-����������!!!");
      }

			// ����� ������ �������������� �� ����� ���������� �� ������������
			// 1. ������������ �������� ������������
			// 2. �����������
			// 3. ���� � ����� ���������� ������������
      // 4. ��� ������������ (����������, ����������� �������� � �.�.)

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

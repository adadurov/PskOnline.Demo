using System;

namespace PskOnline.Math.Statistics
{
  /// <summary>
  /// ��������� ������������� ��������
  /// </summary>
  public class StatData
  {
    public StatData()
    {
    }

    public StatData(StatData src)
    {
      this.m = src.m;
      this.sigma = src.sigma;
      this.dispersion = src.dispersion;
      this.asymmetry = src.asymmetry;
      this.kurtosis = src.kurtosis;
      this.variation = src.variation;
      this.varRange = src.varRange;
      this.min = src.min;
      this.max = src.max;
      this.distribution = new Distribution(src.distribution);
      this.Count = src.Count;
    }

    /// <summary>
    /// �������������� ��������
    /// </summary>
    public double m = 0;

    /// <summary>
    /// �������������������� ����������
    /// </summary>
    public double sigma = 0;

    /// <summary>
    /// ���������, ���������� ������
    /// </summary>
    public double dispersion = 0;

    /// <summary>
    /// �������
    /// </summary>
    public double min = 0;

    /// <summary>
    /// ��������
    /// </summary>
    public double max = 0;

    /// <summary>
    /// ����������� �������� 
    /// </summary>
    public double variation = 0;

    /// <summary>
    /// ������������ ������
    /// </summary>
    public double varRange = 0;

    ///// <summary>
    ///// ����
    ///// </summary>
    //  public double mode = 0;

    ///// <summary>
    ///// ��������� ����
    ///// </summary>
    //  public double modeAmplitude = 0;

    /// <summary>
    /// ����������
    /// </summary>
    public double asymmetry = 0;

    /// <summary>
    /// �������
    /// </summary>
    public double kurtosis = 0;

    /// <summary>
    /// ������������� (�����������)
    /// </summary>
    public Distribution distribution = new Distribution();

    /// <summary>
    /// ��������� �����������
    /// </summary>
    public Distribution probability_density = new Distribution();

    /// <summary>
    /// ���������� ���������
    /// </summary>
    public int Count = 0;

  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Wavelets
{
  /// <summary>
  /// ����� ������������ ���������� ����� ��� ������ �������-����������.
  /// ������� ���� ������������� �����-����� �������������.
  /// ������� ����������� ������ ���� ������� � ���, ��� ��������������
  /// ������������� ������������� �������� ��� ���� � ������� ��������.
  /// ��� ������������� ����������� � ��� ��� ���� ������� ��� ���������
  /// ����� �������, ���� � ���������, ��������, ��������, ��� ��� �������
  /// ��������� � �������������, ����������� ��������������� ���� �
  /// ��������� ����� (haar) ��� fk4.
  /// 
  /// � ������ ���������� ������ ���������� ������� ���������� ���� � �������.
  /// ��������� ������������ ����������, ����������� ���, �������������
  /// (������� ����� ��������� ���������� ����� ��������� � �������),
  /// �� ��� ������ ��� ���������� ������. ���, ��� ������ �������
  /// SetNoiseUniform() ����� ������� ����������� ��������� ���������� ��������
  /// ��������������� �����. ������������� ����������, �� ���������� �
  /// �������������� ������� (���� �� ���, �������), ������������� � ��������,
  /// �����������. ������� ����� ������ ������� ������ ���� ���������� ��
  /// ���������� ����������. � ���������, ��������� ������������� ���������
  /// ����������� ������������� ����������, ��� ��� �������� �� ��������,
  /// ��� ��������� ��� � ���������� ������. ��� ��������� ���� ������ �����
  /// ���������� �������, ������, ����� �� ����� ������ �� ������, ���� ������
  /// ��������� ������������, ��������� �� ���� ������� � ����, �� �� �� �� ���������.
  /// ��������� ������ ������ ����� ������, �� � �����������. ������������ ��������
  /// �������� SetAutothreshold(), �� ���������� ����� ������ ��������.
  /// ����������� �������� ���� �������� 1.0-2.0 .
  /// 
  /// ���������� � ������� ���������� ���������� �� ���������� ����� ��������
  /// ����������. ��� � � ������������ �������, ���� ������� �����������
  /// ���������������� � ���������� �������� ����� �������� AddPoint() �
  /// ��������� ��������� �������� GetPoint(). ����� ���������� ���������� ����������,
  /// ���������� ������� �-� CalculateThresholds(), ������� �������� �����������
  /// ������� � ��������� ��������� ������������. "����������" ��� �������� �� �������
  /// ���� ������ 30 ������� ����� � ������ ������� ������� (���� �� ����������).
  /// ���� ���������� ����������� ����� ������ �������, �� ��� ������ �������� ������,
  /// ������ ��� ����� �������� �������������� ����������. ������������, �����
  /// ��������� ��� ������ ����������, ��� ������� ����� ������� �����.
  /// 
  /// ����� �� ������� ����� ����� �������� ���������� ������� ������� ResetStats(),
  /// ������ ��� ����� ����������/���������� ���, ���������� ��������� � �.�.
  /// </summary>
  public class WaveletAutoDenoiser : WaveletDenoiser
  {
    #region private (implementation details)

    /// <summary>
    /// ������ ������� ��� ����� ����������.
    /// </summary>
    private int stat_size;

    /// <summary>
    /// ��������� �����������
    /// </summary>
    private float[] autothresholds;

    #endregion


    #region public members

    /// <summary>
    /// ������� ���������� �������
    /// </summary>
    public StatCollector[] signal;

    /// <summary>
    /// ������� ���������� ����
    /// </summary>
    public StatCollector[] noise;

    /// <summary>
    /// ��. Wavelet.Wavelet()
    /// </summary>
    /// <param name="level"></param>
    /// <param name="order"></param>
    /// <param name="g"></param>
    public WaveletAutoDenoiser(int level, int order, float[] g)
      : base(level, order, g)
    {
      // FIXME: hardcoded constant
      stat_size = 1024;

      signal = new StatCollector[level];
      noise = new StatCollector[level];
      autothresholds = new float[level];

      for (int i = level - 1; i >= 0; i--)
      {
        signal[i] = new StatCollector();
        signal[i].SetSize(stat_size);
        noise[i] = new StatCollector();
        noise[i].SetSize(stat_size);
        autothresholds[i] = 1.3f;
      }
    }

    /// <summary>
    /// �������� �����. ��. Wavelet.AddPoint(float)
    /// </summary>
    /// <param name="y"></param>
    public override void AddPoint(float y)
    {
      base.AddPoint(y);

      for (int i = level - 1; i >= 0; i--)
      {
        signal[i].AddPoint(w[length * i + position]);
      }
    }

    /// <summary>
    /// �������� ��������� ���������� �������������� �� ��������� ��������� ��������.
    /// </summary>
    /// <param name="quality">����������� �����, ��� ������, ��� ������ �������� ��������� �������� ����� ���������� (������ - �����). �������� ����� 10 ������ ����������.</param>
    public void SetNoiseUniform(int quality)
    {
    	float a=10;
	    int j;
	    int l = GetLatency();
	    int total = l + stat_size*quality;
      System.Random rnd = new Random();
      for( int i = 0; i < total; i++ )
      {
        base.AddPoint((float)(2 * a * (rnd.NextDouble() - 0.5f)));
        if (i >= l)
        {
          for (j = level - 1; j >= 0; j--)
          {
            noise[j].AddPoint(w[length * j + position]);
          }
        }
      }
    }

    /// <summary>
    /// ���������� ���������, �������� ������� ��������� ����.
    /// </summary>
    /// <param name="l">������� ����-��, 0 &lt= l < #level</param>
    /// <param name="t">�������� ����-��, 1 &lt= t; ������������� �������� t=1.3</param>
    public void SetAutoThreshold(int l, float t)
    {
      if ((l < 0) && (l >= level))
      {
        throw new System.ArgumentException(
          $"Value of 'l' is out of range [{0}; {level}."
        );
      }
      autothresholds[l] = t;
    }

    /// <summary>
    /// �������� ���������� �� �������. ���������� ���� �� �������������.
    /// </summary>
    public void ResetStats()
    {
      for (int i = level - 1; i >= 0; i--)
      {
        signal[i].Reset();
      }
    }

    /// <summary>
    /// ��������� � ���������� ��������� ������������ ��� ��������������.
    /// </summary>
    public void CalculateThresholds()
    {
      float t;
      for (int i = level - 1; i >= 0; i--)
      {
        t = signal[i].CalculateThreshold(noise[i], autothresholds[i]);
        SetThreshold(i, t);
      }
    }

    #endregion
  }
}

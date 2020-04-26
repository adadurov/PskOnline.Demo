using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Wavelets
{

  /// <summary>
  /// ����� ������������ ���������� ����� ��� ������ �������-����������, ��������� �������� ���� �������������� �������������.
  /// �� ��������� �������� ������� ����������� � ����. ����� �������������� ����������, ���� ��������� �����-���� ������������� 
  /// �������� ���� ������������� ��� ������ ������� SetThreshold(). ������� 2 ������ ���������� - "������" (�� ���������) � ������.
  /// �������, ��� �������, ���� ������ ����������.
  /// </summary>
  public class WaveletDenoiser : Wavelet
  {
    #region private members

    /// <summary>
    /// ������.
    /// </summary>
    float[] thresholds;

    float tr_k1;
    float tr_a1;
    float tr_k2;
    float tr_a2;

    /// <summary>
    /// �������-��������, �� 2-� ��������: [0,1), [1, +inf)
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private double truncatePolyline(double x)
    {
      if (x < 1.0)
      {
        return tr_k1 * x + tr_a1;
      }

      return tr_k2 * x + tr_a2;
    }

    /// <summary>
    /// 
    /// </summary>
    float tr_power;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    double truncatePower(double x)
    {
      if (x < 1)
      {
        if (x == 0)
        {
          return x;
        }
        return System.Math.Pow(x, (double)tr_power);
      }
      return x;
    }

    /// <summary>
    /// ���������� �������� ����-���
    /// </summary>
    void Truncate()
    {
    	int i,j;
	    float norm;
	    double x;
	
	    for(i=0; i<level; i++)
      {
		    if(thresholds[i] == 0)
        {
			    continue;
        }
		
		    j = i*length + position;
		    norm = w[j] > 0 ? thresholds[i] : -thresholds[i];
		
		    x = w[j]/norm;
		    if( x > (float.MaxValue/2.0) ) // ������������, ��� truncationCurve �� �������� x ����� ��� � 2 ����.
        {
			    continue;
        }
		
		    w[j] = (float)( norm * this.TruncationCurve.Invoke(x) );
	    }
    }

    /// <summary>
    /// ������� ��� ��������
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private delegate double TruncationCurveDelegate(double x);

    /// <summary>
    /// ���������� ��, ��� ����� ���������� �� ��������������� �����������
    /// </summary>
    private TruncationCurveDelegate TruncationCurve;

    #endregion

    #region public members

    /// <summary>
    /// �����������. ��. �������� Wavelet.Wavelet(int, int, float[])
    /// </summary>
    /// <param name="level"></param>
    /// <param name="order"></param>
    /// <param name="g"></param>
    public WaveletDenoiser(int level, int order, float[] g)
      : base(level, order, g)
    {
      thresholds = new float[level];
      for (int i = level - 1; i >= 0; i--)
      {
        thresholds[i] = 0;
      }

      SetModeHard();
      //	latest_is_truncated = 1;
    }

    /// <summary>
    /// ������������� "������" �����
    /// </summary>
    public void SetModeSoft()
    {
      tr_k1 = 0; tr_a1 = 0;
      tr_k2 = 1; tr_a2 = -1;
      this.TruncationCurve = new TruncationCurveDelegate(this.truncatePolyline);
    }

    /// <summary>
    /// ������������� "�������" �����
    /// </summary>
    public void SetModeHard()
    {
      tr_k1 = 0; tr_a1 = 0;
      tr_k2 = 1; tr_a2 = 0;
      this.TruncationCurve = new TruncationCurveDelegate(this.truncatePolyline);
    }

    public void SetModePolyline(float k1, float a1, float k2, float a2)
    {
      tr_k1 = k1; tr_a1 = a1;
      tr_k2 = k2; tr_a2 = a2;
      this.TruncationCurve = new TruncationCurveDelegate(this.truncatePolyline); ;
    }

    /// <summary>
    /// ������������� "�������" �����
    /// </summary>
    /// <param name="p"></param>
    public void SetModePower(float p)
    {
      tr_power = p;
      this.TruncationCurve = new TruncationCurveDelegate(this.truncatePower);
    }

    /// <summary>
    /// ���������� �������� ������. 
    /// ������������ �������� � ���������� ��������� ���� ������ ����� ��������
    /// </summary>
    /// <param name="l">������� ������������� ��������. 0 <= l < level</param>
    /// <param name="t">��������� ��������. 0 <= t</param>
    public void SetThreshold(int l, float t)
    {
    	if( (l<0) || (l>=level) )
      {
		    throw new System.ArgumentException(
		      $"value of 'l' is out of range [{0}; {level})"
		    );
      }

      // It shouldn't be happening but exception is too mean in this case
      if( t < 0 )
      {
        t = 0;
      }

	    thresholds[l] = t;
    }

    /// <summary>
    /// �������� �����. ��. Wavelet.AddPoint(float)
    /// </summary>
    /// <param name="y"></param>
    public override void AddPoint(float y)
    {
      base.AddPoint(y);
    }

    /// <summary>
    /// �������� ��������������� ��������. ��. Wavelet.GetPoint()
    /// </summary>
    /// <returns></returns>
    public override float GetPoint()
    {
      this.Truncate();
      return base.GetPoint();
    }

    #endregion
  }
}

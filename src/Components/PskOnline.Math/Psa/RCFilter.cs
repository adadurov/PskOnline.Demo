namespace PskOnline.Math.Psa
{
  public class RCFilter
  {
    public RCFilter(float SamplingRate, float CutFrequency)
    {
      m_bFirst = true;
      tau = 1;
      RecalculateParams();
      SetValue(0);
    }

    public float filter_point_low(float val)
    {
      if (m_bFirst)
      {
        m_bFirst = false;
        this.SetValue(val);
      }
      this.AddPoint(val);
      return this.GetLow();
    }

    public float filter_point_high(float val)
    {
      if (m_bFirst)
      {
        m_bFirst = false;
        this.SetValue(val);
      }
      this.AddPoint(val);
      return this.GetHigh();
    }

    #region implementation - rewritten in C#
    private bool m_bFirst;
    private double sum;
    private float x;
    private double k;
    private double a;
    private double a_1;
    private float tau;

    /// <summary>
    /// 
    /// </summary>
    void RecalculateParams()
    {
      double a_1_old = a_1;

      k = System.Math.Exp(-1.0 / tau);
      a = 1.0 / (1.0 - k);
      a_1 = 1.0 - k;

      sum *= a_1_old / a_1;
    }

    /// <summary>
    /// ��������� ��������� ��������.
    /// ��� ������������� ������� �� ����� �� ������ ������� ��������,
    /// ��� ����� �� ��� �� ���� ���������� ����� ��������� ������� ������.
    /// ������ ������� ���������� ������ 0 ������������ �������� y.
    /// �� ����� �������� � ����� �����.
    /// </summary>
    /// <param name="y"></param>
    void SetValue(float y)
    {
      x = y;
      sum = y * a;
    }

    /// <summary>
    /// ������������� ����� ���������� RC-���� ������ tau.
    /// </summary>
    /// <param name="tau"></param>
    void SetTau(float tau)
    {
      this.tau = tau;
      RecalculateParams();
    }

    /// <summary>
    /// ������������� ����� ���������� �����, ��� ���������� � 1/sqrt(2) ���������� �� ������� w.
    /// </summary>
    /// <param name="w"></param>
    void SetThresholdFrequency(float w)
    {
      SetTau((float)(1 / (2 * System.Math.PI * w)));
    }

    /// <summary>
    /// �������� �����.
    /// </summary>
    /// <param name="y"></param>
    void AddPoint(float y)
    {
      x = y;
      sum = sum * k + x;
    }

    /// <summary>
    /// �������� �� ����� ������� (��� ���� �� ��� ��� ���).
    /// </summary>
    /// <returns></returns>
    float GetLow()
    {
      return (float)(sum * a_1);
    }

    /// <summary>
    /// �������� �� ����� ������� (��� ���� �� ��� ��� ���).
    /// </summary>
    /// <returns></returns>
    float GetHigh()
    {
      return (float)((x - sum * a_1) * a_1 * tau / k);
    }

    #endregion
  }
}

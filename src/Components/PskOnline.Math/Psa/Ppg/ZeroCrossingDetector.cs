using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{

  /// <summary>
  /// ��������� ����������� ����������� �������� ������
  /// </summary>
  public enum ZeroCrossingDirection
  {
    /// <summary>
    /// ����� �����
    /// </summary>
    Up,

    /// <summary>
    /// ������ ����
    /// </summary>
    Down
  }


  /// <summary>
  /// ����������� ������������������ �������� ������ � ����� ������ ����� ����������� ����.
  /// ���� ����������� ������������ ����� �������� ������������ ����� �������, �������� �� ������ ������� ����.
  /// </summary>
  public class ZeroCrossingDetector
  {
    public ZeroCrossingDetector(ZeroCrossingDirection direction)
    {
      m_direction = direction;
    }

    private ZeroCrossingDirection m_direction;
    private int count = 0;

    private double m_lastZeroCrossingPoint = 0;

    private double previous_nonzero_value = 0;

    private int previous_nonzero_value_distance = 0;

    private double previous_value = 0;
    private bool zero_approached = false;


    internal bool CheckValuesAreOnDifferentSidesOfZero(double val1, double val2)
    {
      return ( (val1 < 0) && (val2 > 0) ) || ((val1 > 0) && (val2 < 0));
    }

    /// <summary>
    /// ���������� ���� ������� ����������� ���� ����� ������� ��� ������ ��������.
    /// ��� ����������� ��������� ����� ����������� ������� �������� ������� GetLastZeroCrossingPoint().
    /// </summary>
    /// <param name="cur_val">����� �������� ������������������ ������� ������</param>
    /// <returns>���������� ������� ����������� ����� ����������� ���� + </returns>
    public bool AddValue(double cur_val)
    {
      if (UpdateWithNewPoint(cur_val))
      {
        return true;
      }
      else
      {
        // � ����������� ����� ����� ���� �� 1 �� ���������� ����� ����������� ����
        this.m_lastZeroCrossingPoint -= 1.0;
        return false;
      }

    }

    private bool UpdateWithNewPoint(double cur_val)
    {
      try
      {
        ++count;
        if (count > 1)
        {

          // ����� ����������� "0"?
          if (CheckValuesAreOnDifferentSidesOfZero(previous_value, cur_val))
          {
            previous_nonzero_value = previous_value;
            zero_approached = true;
            previous_nonzero_value_distance = -1;
          }

          // ������������ � "0" ������?
          if (zero_approached)
          {
            if (CheckValuesAreOnDifferentSidesOfZero(previous_nonzero_value, cur_val))
            {
              // ��, ���� �����������
              // ��������� ���������...

              double y1 = (double)previous_nonzero_value;
              double y2 = (double)cur_val;

              double x1 = (double)this.previous_nonzero_value_distance;
              double x2 = (double)0;

              System.Diagnostics.Debug.Assert(y2 != y1);

              this.m_lastZeroCrossingPoint = x1 - y1 * (x2 - x1) / (y2 - y1);


              // ����� ������, �����, ��� ���� ��������� �����������
              // �, � ����������� �� ���������� ����������� � ������������ ���
              // ������� true ��� false.

              zero_approached = false;

              if ((this.previous_nonzero_value > 0) && (this.m_direction == ZeroCrossingDirection.Down))
              {
                // ����������� ���� ��������� ������ ����, ��� ��� � ���������
                return true;
              }
              if ((this.previous_nonzero_value < 0) && (this.m_direction == ZeroCrossingDirection.Up))
              {
                // ����������� ���� ��������� ����� �����, ��� ��� � ���������
                return true;
              }

              return false;
            }
            else if (cur_val == 0)
            {
              // �������� � "0"
              --this.previous_nonzero_value_distance;
              return false;
            }
            else
            {
              // ���� �� "0" � �� �� �������, ������ ������
              // �������� ���� ����������� � "0"
              zero_approached = false;
              return false;
            }

          }

          if ((cur_val == 0) && (this.previous_value != 0))
          {
            // zero approached
            this.zero_approached = true;
            this.previous_nonzero_value = previous_value;
            this.previous_nonzero_value_distance = -1;
          }


        }
      }
      finally
      {
        previous_value = cur_val;
      }
      return false;
    }

    /// <summary>
    /// ���������� ���������� �� ���������� ������������ ������� �� ����� ����������� ����.
    /// </summary>
    /// <returns></returns>
    public double GetLastZeroCrossingPoint()
    {
      return m_lastZeroCrossingPoint;
    }


    public bool TestSeries(double[] series)
    {
      bool bZeroCrossingDetected = false;
      for (int i = 0; i < series.Length; ++i)
      {
        bZeroCrossingDetected |= this.AddValue(series[i]);
      }
      return bZeroCrossingDetected;
    }

  }
}

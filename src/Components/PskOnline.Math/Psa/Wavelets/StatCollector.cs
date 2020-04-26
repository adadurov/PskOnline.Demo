using System;

namespace PskOnline.Math.Psa.Wavelets
{
  public class StatCollector
  {
    #region public

    public StatCollector()
    {
      n = 0;
      stat_max = 0;
      stat = null;
      linear_part = 0.5F;
      idx = 1;
      is_cumulative = false;
    }

    /// <summary>
    /// �������� �������� x, ��� ������� �-� �������. ������������ ��������� �������� f.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public float GetXbyF(float f)
    {
      float r;

      ConvertToCumulative();

      if (idx <= 0)
      {
        idx = 1;
      }
      else if (idx >= size - 1)
      {
        idx = size - 2;
      }

      f *= n;

      while ((idx > 0) && (idx < size - 2))
      {
        if (stat[idx + 1] < f)
        {
          idx += 1;
        }
        else if (stat[idx] > f)
        {
          idx -= 1;
        }
        else
        {
          break;
        }
      }

      if (stat[idx] == stat[idx + 1])
      {
        r = idx + 0.5f;
      }
      else
      {
        r = idx + (f - stat[idx]) / (stat[idx + 1] - stat[idx]);
      }

      return r * stat_max / size;
    }

    /// <summary>
    /// ���������� ���-�� ��������, ������������ ��� ���������� �������������.
    /// ��� �-� ����������� ������ ���� ������� ����� ���������� ��������������.
    /// </summary>
    /// <param name="s"></param>
    public void SetSize(int s)
    {
      if (((s % 2) != 0) || (s <= 0))
      {
        throw new System.ArgumentException("StatCollector size must be multiple of 2 and >0.");
      }

      size = s;
      stat = new int[size];
      Reset();
    }

    /// <summary>
    /// �������� ����������
    /// </summary>
    public void Reset()
    {
      n = 0;
      stat_max = 0;
      idx = 1;
      is_cumulative = false;
      for (int i = stat.Length - 1; i >= 0; --i)
      {
        stat[i] = 0;
      }
    }

    /// <summary>
    /// �������� ����� ��������� ��������� ��������.
    /// </summary>
    /// <param name="x"></param>
    public void AddPoint(float y)
    {
      ConvertToDensity();

      y = System.Math.Abs(y);

      if (y > float.MinValue * 2)
      {
        if (stat_max == 0)
        {
          stat_max = y;
        }
      }
      else
      {
        stat[0]++;
        return;
      }

      while (stat_max < y)
      {
        Scale2();
      }

      int i = (int)(size * y / stat_max);

      if (i < 0) i = 0;		// Hm... this shold never happen... 
      if (i >= size) i = size - 1; // This either

      stat[i]++;
      n++;
    }

    /// <summary>
    /// ������������� ��������� ��������� ������� ������� �������� ��������� ��������.
    /// </summary>
    /// <param name="m"></param>
    public void SetMaxValue(float m)
    {
      stat_max = m;
    }

    /// <summary>
    /// ������������� ��������� ����������� � ������������� �����������.
    /// </summary>
    void ConvertToCumulative()
    {
      if (is_cumulative)
      {
        return;
      }

      is_cumulative = true;

      for (int i = 1; i < size; i++)
      {
        stat[i] += stat[i - 1];
      }
    }

    /// <summary>
    /// ������������� ������������� ����������� � ��������� �����������.
    /// </summary>
    public void ConvertToDensity()
    {
      if( ! is_cumulative )
      {
        return;
      }

      is_cumulative = false;

      for (int i = size - 1; i > 0; i--)
      {
        stat[i] -= stat[i - 1];
      }
    }

    /// <summary>
    /// ��������� ��������� ��������, �� ������� ������ ��������� ���������.
    /// </summary>
    /// <param name="noise">
    /// ������, ���������� ����. ������������� ����
    /// </param>
    /// <param name="t">
    /// ��������, ��������� ������ �������,
    /// ���������� � ������������ � ������ ������� � ����,
    /// ����������� �����������.</param>
    /// <returns>Threshold value</returns>
    public float CalculateThreshold(StatCollector noise, float t)
    {
      float ks, kn;
      if (size != noise.size)
      {
        // FIXME: ��� ����������� ����� �����.
        throw new ArgumentException("noise.size must be equal to this.size!");
      }

      // ���, �������, �����������, �� exception ��������� �� �����
      if ((n < 1) || (noise.n < 1))
      {
        return 0;
      }

      // ����������...
      if ((stat_max == 0) || (noise.stat_max == 0)) 
      {
        return 0;
      }

      ks = CalculateSlope();       // This can't be 0
      kn = noise.CalculateSlope(); // So is this

      //  Console.Error.WriteLine("slope: s={0}, n={1}", ks, kn);
      //  Console.Error.WriteLine("smax : s={0}, n={1}", stat_max, noise.stat_max);
      int i, sum;
      float f, x;

      ConvertToDensity();
      sum = n;
      for (i = size - 1; i > 0; i--)
      {
        f = (float)sum / n;
        x = noise.GetXbyF(f);

        //  Console.Error.WriteLine("{0} {1} {2} {3} {4}", ks*(i*stat_max/size), kn*x, x, f);
        if (ks * (i * stat_max / size) < kn * x * t)
        {
          break;
        }

        sum -= stat[i];
      }
      return i * stat_max / size;
    }

    /// <summary>
    /// ������� ���������� � ����.
    /// </summary>
    /// <param name="filename"></param>
    public void DumpToFile(string filename)
    {
    }

    #endregion

    #region private

    int size;
    int[] stat;
    float stat_max;
    int n;

    /// <summary>
    /// ������ ��� ��������� ������� GetXbyF
    /// </summary>
    int idx;

    float linear_part;

    /// <summary>
    /// �� 0, ���� ������������� ���� ������������� ������������.
    /// </summary>
    bool is_cumulative;

    /// <summary>
    /// ��������� ������� ������� � 2 ����
    /// </summary>
    void Scale2()
    {
      int i;
      ConvertToDensity();
      for (i = 0; i < size / 2; i++)
      {
        stat[i] = stat[2 * i] + stat[2 * i + 1];
      }

      for (i = size / 2; i < size; i++)
      {
        stat[i] = 0;
      }

      stat_max *= 2;
    }

    /// <summary>
    /// ��������� ������ ������ ������� ������������� ������������ �� ������ �������� �������������.
    /// ����������, ����� ������ ���� ������������� � ���� ��������� �����������.
    /// </summary>
    /// <returns></returns>
    float CalculateSlope()
    {
      int i, sum = 0;
      double sxy = 0, sxx = 0;
      for (i = 0; i < size; i++)
      {
        if (is_cumulative)
        {
          sum = stat[i];
        }
        else
        {
          sum += stat[i];
        }

        sxx += (i + 0.5) * (i + 0.5);
        sxy += (i + 0.5) * sum;

        if (sum > n * linear_part)
        {
          break;
        }
      }

      return (float)((sxy / sxx) * (size / stat_max) / n);
    }

    #endregion
  }
}

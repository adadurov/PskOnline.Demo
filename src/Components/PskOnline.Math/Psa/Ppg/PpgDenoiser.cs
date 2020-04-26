using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{
  public class PpgDenoiser
  {
    bool m_bHistoryEmpty = true;
    int[] m_History = null;

    int m_Length = 0;
    int m_Begin = 0;
    int m_End = 0;

    long m_Sum = 0;

    int k = 17;

    public PpgDenoiser(double sampling_rate)
    {
      m_Length = (int)(sampling_rate / k);
      m_History = new int[m_Length];
    }

    public void FilterInPlace(int[] data)
    {
      if (this.m_bHistoryEmpty)
      {
        for (int i = 0; i < m_Length; ++i)
        {
          m_History[i] = data[0];
          m_Begin = 0;
          m_End = m_Length - 1;
          m_Sum += data[0];
        }
        this.m_bHistoryEmpty = false;
      }
      // begin filtering

      for (int i = 0; i < data.Length; ++i)
      {
        m_Sum -= m_History[this.m_Begin];
        this.m_Sum += data[i];
        m_History[this.m_End] = data[i];

        data[i] = (int)(m_Sum / m_Length);

        IncrementHistoryPointers();
      }

      return;
    }

    void IncrementHistoryPointers()
    {
      if (++this.m_Begin >= this.m_Length)
      {
        this.m_Begin = 0;
      }
      if (++this.m_End >= this.m_Length)
      {
        this.m_End = 0;
      }
    }

  }
}

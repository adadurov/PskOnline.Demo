using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{
  internal class PpgDispersionNormalizerParams
  {
    internal PpgDispersionNormalizerParams(long DSN_, int KA_min_, int KA_max_)
    {
      this.m_DSN = DSN_;
      this.m_KA_min = KA_min_;
      this.m_KA_max = KA_max_;
    }

    private long m_DSN = 2073;

    public long DSN
    {
      get
      {
        return this.m_DSN;
      }
    }


    private int m_KA_min = 400;

    public int KA_min
    {
      get
      {
        return this.m_KA_min;
      }
    }

    private int m_KA_max = 13000;

    public int KA_max
    {
      get
      {
        return this.m_KA_max;
      }
    }


  }
}

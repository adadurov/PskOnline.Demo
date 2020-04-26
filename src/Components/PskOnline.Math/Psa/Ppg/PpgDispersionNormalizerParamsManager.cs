using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ppg
{
  internal static class PpgDispersionNormalizerParamsManager
  {
    public static PpgDispersionNormalizerParams GetParams(int BitsPerSample)
    {
      InitParams();

      System.Diagnostics.Debug.Assert(m_ParamsStorage.Count > 0);

      if (m_ParamsStorage.ContainsKey(BitsPerSample))
      {
        return m_ParamsStorage[BitsPerSample];
      }
      else
      {
        string warning =
          $"Precise PPG dispersion normalizer parameters not specified for '{BitsPerSample}' bits per sample.";

#if DEBUG
        System.Diagnostics.Debug.Fail( warning );
#endif
        // возвращаем ближайшее к требуемому с предупреждением!!!
        logger.Warn(warning);

        Dictionary<int, PpgDispersionNormalizerParams>.KeyCollection.Enumerator kEnum = m_ParamsStorage.Keys.GetEnumerator();

        int min_diff = int.MaxValue;
        int cur_diff;
        int closest_key = 0;

        while (kEnum.MoveNext())
        {
          cur_diff = System.Math.Abs(kEnum.Current - BitsPerSample);
          if (min_diff > cur_diff)
          {
            closest_key = kEnum.Current;
            min_diff = cur_diff;
          }
        }

        return m_ParamsStorage[closest_key];
      }
    }

    static Dictionary<int, PpgDispersionNormalizerParams> m_ParamsStorage = new Dictionary<int, PpgDispersionNormalizerParams>(5);

    static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(PpgDispersionNormalizerParamsManager));

    private static void InitParams()
    {
      if (m_ParamsStorage.Count == 0)
      {
        m_ParamsStorage[8] = new PpgDispersionNormalizerParams(2373, 400, 13000);
        m_ParamsStorage[10] = new PpgDispersionNormalizerParams(2373, 400, 13000);
      }
    }

  }
}

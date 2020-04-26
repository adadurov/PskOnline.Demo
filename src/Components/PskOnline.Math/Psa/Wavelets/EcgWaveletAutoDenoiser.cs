using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Wavelets
{
  public class EcgWaveletAutoDenoiser
  {
    public EcgWaveletAutoDenoiser()
    {
      InitNew();
      count = 0;
    }

    WaveletAutoDenoiser wad = null;
    int count;

    private void InitOld()
    {
      float[] g = new float[] {0.7071067811865474617150f, 0.7071067811865474617150f};
      int level = 5;
      int order = 2;
      
      wad = new WaveletAutoDenoiser(level, order, g);

      wad.SetNoiseUniform(10);
      wad.SetAutoThreshold(0, 1.1f);
      wad.SetAutoThreshold(1, 1.3f);
      wad.SetAutoThreshold(2, 1.3f);
      wad.SetAutoThreshold(3, 1.3f);
      wad.SetAutoThreshold(4, 1.3f);
    }

    private void InitNew()
    {
      float[] g = {
                    0.65392755556976511766010f,  0.75327249283948716218617f,
                    0.05317922877905981171587f, -0.04616571481521770242695f
                  };
      
      wad = new WaveletAutoDenoiser(5, 4, g);

      wad.SetNoiseUniform(10);
      wad.SetAutoThreshold(0, 2.2f);
      wad.SetAutoThreshold(1, 2.2f);
      wad.SetAutoThreshold(2, 2.2f);
      wad.SetAutoThreshold(3, 2.2f);
      wad.SetAutoThreshold(4, 2.2f);

      // По умолчанию стоит режим фильтрации Hard, но были пожелания оставлять немного шума. Для этого можно использовать 
      wad.SetModePolyline(0.05f, 0, 1, 0);

      // Тут пропускается 5% от исходного шума
    }

    public void FilterDestructive(int[] signal)
    {
      for( int i = 0; i < signal.Length; ++ i )
      {
        wad.AddPoint((float)signal[i]);
        signal[i] = (int)wad.GetPoint();

        if( (++count)%1000 == 999 )
        {
          wad.CalculateThresholds();
          wad.ResetStats();
        }
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Ecg
{
  public class EcgAnalyzerFactory
  {
    public static EcgAnalyzer MakeSimpleAnalyzer(double SamplingRate)
    {
      return new EcgAnalyzer(SamplingRate);
    }

    public static AdvancedEcgAnalyzer MakeAdvancedAnalyzer(double SamplingRate)
    {
      return new AdvancedEcgAnalyzer(SamplingRate);
    }
  }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.Processing.Logic
{
  using PskOnline.Methods.Hrv.ObjectModel;

  class PpgSpectrumAnalyzer
  {
    /// <summary>
    /// вычисляет спектр сигнала
    /// </summary>
    /// <param name="spectrum"></param>
    /// <returns></returns>
    public static Spectrum CalculateSpectrum(int[] signal, double SamplingRate)
    {
      if( signal.Length < 1 )
      {
        throw new ArgumentException();
      }

      //signal = DownsampleSignal(signal);
      //signal = ResampleSignalToPowerOf2(signal, ref SamplingRate);

      //signal = GenerateSinus(signal, SamplingRate);
      //signal = GenerateMeander(signal, SamplingRate);

      int length = 1;
      while (((length * 2) < signal.Length) && ((2 * length) < (512 * 1024)) )
      {
        length *= 2;
      }

      // remove constant
      signal = CenterSignal(signal, length);

      Exocortex.DSP.Complex[] complex_data = new Exocortex.DSP.Complex[length];
      for( int i = 0; i < length; ++i )
      {
        complex_data[i].Re = signal[i];
        complex_data[i].Im = 0;
      }

      Exocortex.DSP.Fourier.FFT(complex_data, length, Exocortex.DSP.FourierDirection.Forward);

      // calculate power
      Spectrum result = new Spectrum();
      result.FreqResolution = SamplingRate / 2 / ( ((double)length) / 2);
      result.SpectrumLow = 0;
      result.SpectrumHigh = SamplingRate / 2;

      for (int k = 0; k < (length / 2); ++k)
      {
        result.SpectrumBins.Add(
          System.Math.Sqrt(
            complex_data[k].Re * complex_data[k].Re +
            complex_data[k].Im * complex_data[k].Im
          )
        );
      }

      return result;
    }

    private static int[] CenterSignal(int[] signal, int length)
    {
      long sum = 0L;
      for (int i = 0; i < length; ++i)
      {
        sum += signal[i];
      }

      int ave = (int)(sum / length);

      int[] result = new int[length];
      for (int i = 0; i < length; ++i)
      {
        result[i] = signal[i] - ave;
      }

      return result;
    }

    /// <summary>
    /// Generates sinus signal at 1 Hz
    /// </summary>
    /// <param name="signal"></param>
    /// <returns></returns>
    private static int[] GenerateSinus(int[] signal, double SamplingRate)
    {
      int size = 4096;
      int[] new_signal = new int[4096];

      double frequency = 1;

      for (int i = 0; i < size; ++i)
      {
        new_signal[i] = 128 + (int)(100.0 * System.Math.Sin(frequency * 2.0 * System.Math.PI * ((double)i) / SamplingRate));
      }
      return new_signal;
    }

    /// <summary>
    /// Generates meander signal at 1 Hz
    /// </summary>
    /// <param name="signal"></param>
    /// <returns></returns>
    private static int[] GenerateMeander(int[] signal, double SamplingRate)
    {
      return new int[0];
    }

  }
}

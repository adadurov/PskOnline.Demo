namespace PskOnline.Methods.Hrv.ObjectModel
{
  using System.Collections.Generic;

  /// <summary>
  /// Результаты спектрального анализа 
  /// </summary>
  public class HrvSpectrumResult : Spectrum
  {
    public HrvSpectrumSource HrvSpectrumSource = null;

    public BaevskiIndices BaevskiIndices = new BaevskiIndices();

    /// <summary>
    /// HF total power
    /// </summary>
    public double HFTP = 0;

    /// <summary>
    /// нижная граница диапазона HF
    /// </summary>
    public double HF_LowBound = 0.15;

    /// <summary>
    /// верхняя граница диапазона HF
    /// </summary>
    public double HF_HighBound = 0.4;

    /// <summary>
    /// LF total power
    /// </summary>
    public double LFTP = 0;

    /// <summary>
    /// нижная граница диапазона HF
    /// </summary>
    public double LF_LowBound = 0.04;

    /// <summary>
    /// верхняя граница диапазона HF
    /// </summary>
    public double LF_HighBound = 0.15;

    /// <summary>
    /// VLF total power
    /// </summary>
    public double VLFTP = 0;

    /// <summary>
    /// нижная граница диапазона HF
    /// </summary>
    public double VLF_LowBound = 0.01;

    /// <summary>
    /// верхняя граница диапазона HF
    /// </summary>
    public double VLF_HighBound = 0.04;

    /// <summary>
    /// ULF total power
    /// </summary>
    public double ULFTP = 0;

    /// <summary>
    /// нижная граница диапазона HF
    /// </summary>
    public double ULF_LowBound = 0.0001;

    /// <summary>
    /// верхняя граница диапазона HF
    /// </summary>
    public double ULF_HighBound = 0.01;
  }


}

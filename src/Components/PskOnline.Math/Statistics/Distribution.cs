using System;

namespace PskOnline.Math.Statistics
{
  /// <summary>
  /// Распределение (гистограмма).
  /// 
  /// Первый диапазон (channels[0]) содержит вероятность
  /// попадания значения данной реализации случайной функции
  /// в диапазон [min, min + band_width), второй диапазон (channels[1]) --
  /// вероятность попадания в диапазон [min + band_width, min + 2 * band_width) и т.д.
  /// при условии, что все значения данной реализации случайной функции
  /// находятся в диапазоне [min, max].
  /// </summary>
  public class Distribution
	{
		public Distribution()
		{
		}

		public Distribution(Distribution src)
		{
			min = src.min;
			max = src.max;
      mode = src.mode;
      mode_amplitude = src.mode_amplitude;
			channel_count = src.channel_count;
      channel_width = src.channel_width;
			if( src.channels != null )
			{
				this.channels = new double[src.channels.GetLength(0)];
        src.channels.CopyTo(this.channels, 0);
			}
		}

    /// <summary>
    /// Начало гистограммы
    /// </summary>
		public double min = 0;

    /// <summary>
    /// Конец гистограммы
    /// </summary>
		public double max = 0;
    
    /// <summary>
    /// Количество диапазонов гистограммы
    /// </summary>
		public int	  channel_count = 0;

    /// <summary>
    /// Ширина диапазона гистограммы
    /// </summary>
    public double channel_width = 0;

    /// <summary>
    /// "Наиболее вероятное значение"
    /// </summary>
    public double mode = 0;

    /// <summary>
    /// плотность вероятности, соответствующая
    /// "наиболее вероятному значению"
    /// </summary>
    public double mode_amplitude = 0;

    /// <summary>
    /// Каналы
    /// </summary>
		public double[] channels;
	}
}

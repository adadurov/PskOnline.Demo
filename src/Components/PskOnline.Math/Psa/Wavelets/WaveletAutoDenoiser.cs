using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Wavelets
{
  /// <summary>
  /// Класс осуществляет подавление шумов при помощи вэйвлет-фильтрации.
  /// Уровень шума отслеживается более-менее автоматически.
  /// Принцип определения уровня шума состоит в том, что статистические
  /// распределения коэффициентов вэйвлета для шума и сигнала различны.
  /// Это предположение справедливо в той или иной степени для различных
  /// типов сигнала, шума и вэйвлетов, известно, например, что оно неплохо
  /// применимо к кардиограммам, однородному распределенному шуму и
  /// вэйвлетам Хаара (haar) или fk4.
  /// 
  /// В каждом конкретном случае необходимо набрать статистику шума и сигнала.
  /// Поскольку табулировать статистику, описывающую шум, нерационально
  /// (слишком много возможных комбинаций типов вэйвлетов и уровней),
  /// то она каждый раз собирается заново. Так, при вызове функции
  /// SetNoiseUniform() через вэйвлет прогоняется некоторое количество случайно
  /// сгенерированных чисел. Статистистика набирается, но информация о
  /// предшествующем сигнале (если он был, конечно), содержавшаяся в вэйвлете,
  /// разрушается. Поэтому вызов данной функции должен быть произведен ДО
  /// проведения фильтрации. К сожалению, полностью автоматически выполнять
  /// сглаживание принципиально невозможно, так как априорно не известно,
  /// где кончается шум и начинается сигнал. Тут требуется либо ввести кусок
  /// эталонного сигнала, такого, какой мы хотим видеть на выходе, либо задать
  /// некоторые коэффициенты, зависящие от типа сигнала и шума, но не от их амплитуды.
  /// Поскольку второй способ более удобен, он и осуществлен. Коэффициенты вводятся
  /// функцией SetAutothreshold(), их количество равно уровню вэйвлета.
  /// Хакактерные значения этих констант 1.0-2.0 .
  /// 
  /// Статистика о сигнале собирается непрерывно на протяжении всего процесса
  /// фильтрации. Как и в родительских классах, этот процесс заключается
  /// последовательном в добавлении входного числа функцией AddPoint() и
  /// забирании выходного функцией GetPoint(). Когда набирается достаточно статистики,
  /// необходимо вызвать ф-ю CalculateThresholds(), которая проведет необходимые
  /// расчеты и устоновит пороговые коэффициенты. "Достаточно" тут означает по крайней
  /// мере больше 30 входных точек и больше периода сигнала (если он периодичен).
  /// Если количество накопленных точек меньше десятка, то эту фунцию вызывать нельзя,
  /// потому как можно получить неопределенные результаты. Впоследствии, когда
  /// накопится еще больше статистики, эту функцию можно вызвать снова.
  /// 
  /// Время от времени имеет смысл обнулять статистику сигнала вызовом ResetStats(),
  /// потому как может увеличится/уменьшится шум, измениться амплитуда и т.п.
  /// </summary>
  public class WaveletAutoDenoiser : WaveletDenoiser
  {
    #region private (implementation details)

    /// <summary>
    /// размер массива для сбора статистики.
    /// </summary>
    private int stat_size;

    /// <summary>
    /// константы сглаживания
    /// </summary>
    private float[] autothresholds;

    #endregion


    #region public members

    /// <summary>
    /// сборщик статистики сигнала
    /// </summary>
    public StatCollector[] signal;

    /// <summary>
    /// сборщик статистики шума
    /// </summary>
    public StatCollector[] noise;

    /// <summary>
    /// см. Wavelet.Wavelet()
    /// </summary>
    /// <param name="level"></param>
    /// <param name="order"></param>
    /// <param name="g"></param>
    public WaveletAutoDenoiser(int level, int order, float[] g)
      : base(level, order, g)
    {
      // FIXME: hardcoded constant
      stat_size = 1024;

      signal = new StatCollector[level];
      noise = new StatCollector[level];
      autothresholds = new float[level];

      for (int i = level - 1; i >= 0; i--)
      {
        signal[i] = new StatCollector();
        signal[i].SetSize(stat_size);
        noise[i] = new StatCollector();
        noise[i].SetSize(stat_size);
        autothresholds[i] = 1.3f;
      }
    }

    /// <summary>
    /// добавить точку. См. Wavelet.AddPoint(float)
    /// </summary>
    /// <param name="y"></param>
    public override void AddPoint(float y)
    {
      base.AddPoint(y);

      for (int i = level - 1; i >= 0; i--)
      {
        signal[i].AddPoint(w[length * i + position]);
      }
    }

    /// <summary>
    /// Провести измерение равномерно распределенной на интервале случайной величины.
    /// </summary>
    /// <param name="quality">натуральное число, чем больше, тем больше значений случайной величины будет обработано (больше - лучше). Значения около 10 вполне достаточно.</param>
    public void SetNoiseUniform(int quality)
    {
    	float a=10;
	    int j;
	    int l = GetLatency();
	    int total = l + stat_size*quality;
      System.Random rnd = new Random();
      for( int i = 0; i < total; i++ )
      {
        base.AddPoint((float)(2 * a * (rnd.NextDouble() - 0.5f)));
        if (i >= l)
        {
          for (j = level - 1; j >= 0; j--)
          {
            noise[j].AddPoint(w[length * j + position]);
          }
        }
      }
    }

    /// <summary>
    /// Выставляет константы, задающие степень обрезания шума.
    /// </summary>
    /// <param name="l">уровень коэф-та, 0 &lt= l < #level</param>
    /// <param name="t">значение коэф-та, 1 &lt= t; рекомендуемое значение t=1.3</param>
    public void SetAutoThreshold(int l, float t)
    {
      if ((l < 0) && (l >= level))
      {
        throw new System.ArgumentException(
          $"Value of 'l' is out of range [{0}; {level}."
        );
      }
      autothresholds[l] = t;
    }

    /// <summary>
    /// Обнуляет статистику по сигналу. Статистика шума не затрагивается.
    /// </summary>
    public void ResetStats()
    {
      for (int i = level - 1; i >= 0; i--)
      {
        signal[i].Reset();
      }
    }

    /// <summary>
    /// Вычислить и установить пороговые коэфиициенты для шумоподавления.
    /// </summary>
    public void CalculateThresholds()
    {
      float t;
      for (int i = level - 1; i >= 0; i--)
      {
        t = signal[i].CalculateThreshold(noise[i], autothresholds[i]);
        SetThreshold(i, t);
      }
    }

    #endregion
  }
}

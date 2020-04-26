using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Wavelets
{
  /// <summary>
  /// Класс, осуществляющий вэйвлет-преобразование (неортогональное, инвариантное к сдвигу, избыточное, undecimated).
  /// Преобразование данного типа может быть пригодно для разложения сигнала в режиме реального времени.
  /// 
  /// У преобразования имеется имеется два основных параметра: уровень и дискретная ф-я масштабирования вэйвлета. 
  /// Ф-ии масштабирования выбираются из некоторого множества фунций, найденных специалистами по вэйвлетам исходя из 
  /// каких-либо соображений. Уровень - положительное целое число - выбирается в соответствии с типом обрабатываемого
  /// сигнала. Чем выше уровень, тем больший временной кусок сигнала захватывает преобразование, давая возможность
  /// анализировать более низкочастотные компоненты сигнала. Одновременно с этим растет задержка между входом и 
  /// выходом фильтра.
  /// 
  /// Обработка информации организована в режиме FIFO: на вход подается точка, с выхода забирается точка.
  /// Количество вычислительных ресурсов, требуемое на разложение и реконструкцию, примерно одинаково, поэтому 
  /// восстановление сделано опциональным, т.е. значения с выхода можно не забирать. Если же выходные значения нужны,
  /// то забирать их надо НЕПРЕРЫВНО. После перерыва нормальные выходные значения появляются только через количество точек, 
  /// равное задержке.
  /// </summary>
  public class Wavelet
  {
    #region private & protected

    /// <summary>
    /// Входной сигнал 1:1
    /// </summary>
		private float[] x;  
    
    /// <summary>
    /// НЧ компоненты
    /// </summary>
    private float[] v;  

    /// <summary>
    /// Коэф-ты для реконструкции
    /// </summary>
    private float[] iv; 

    #endregion
    
    #region protected 
		
    /// <summary>
    /// ВЧ коэф-ты разложения
    /// </summary>
    protected float[] w;

    /// <summary>
    /// Ф-я вэйвлета
    /// </summary>
    protected float[] h;

    /// <summary>
    /// Ф-я масштабирования
    /// </summary>
    protected float[] g;

    /// <summary>
    /// Уровень вэйвлета
    /// </summary>
    protected int level;

    /// <summary>
    /// Индекс ф-ии масштабирования (равен ее длине)
    /// </summary>
    protected int n;

    /// <summary>
    /// Длина циклических массивов
    /// </summary>
    protected int length;

    /// <summary>
    /// Текущее положение
    /// </summary>
    protected int position;

    /// <summary>
    /// Освободить все выделенные области памяти.
    /// </summary>
    protected void CleanupMemory()
    {
    }

    #endregion

    #region public

    /// <summary>
    /// Инициализатор вэйвлета
    /// </summary>
    /// <param name="level">уровень вэйвлета</param>
    /// <param name="n">индекс вэйвлета (совпадает с длиной)</param>
    /// <param name="g">массив точек ф-ии масштабирования длиной idx</param>
    public Wavelet(int level, int n, float[] g)
    {
      int i;

      this.level = level;
      this.n = n;

      length = ((1 << (level)) - 1) * (n - 1) + 1;
      position = 0;

      this.x = new float[length];
      this.v = new float[length * level];
      this.w = new float[length * level];
      this.iv = new float[length * level];
      this.g = new float[n];
      this.h = new float[n];

      Reset();

      for (i = n - 1; i >= 0; i--)
      {
        h[i] = (i % 2 == 0 ? 1 : -1) * g[n - 1 - i];
        this.g[i] = g[i];
      }
    }

    /// <summary>
    /// Обнулить внутренние буферы.
    /// </summary>
    public void Reset()
    {
      int i;
      for (i = length - 1; i >= 0; i--)
        x[i] = 0;

      for (i = length * level - 1; i >= 0; i--)
      {
        v[i] = 0;
        w[i] = 0;
        iv[i] = 0;
      }
    }

    /// <summary>
    /// Добавить новую точку.
    /// </summary>
    /// <param name="val">значение</param>
    public virtual void AddPoint(float val)
    {
      float[] da;
      int d;

      float ws, vs;
      int i, j, p1, step, l;

      position++;
      if (position >= length)
      {
        position -= length;
      }

      // Добавить новую точку
      x[position] = val;

      // Выставить указатель на рабочий массив
      d = 0;
      da = this.x;

      // Здесь i определяет текущий шаг как 2^i. Разложение начинается с самой ВЧ части
      // В цикле считаются и сохраняются коэф. вэйвлета, и перемещается указатель на рабочий массив
      for (i = 0; i < level; i++)
      {
        step = 1 << i;
        p1 = position;
        ws = 0;
        vs = 0;
        for (j = 0; j < n; j++)
        {
          ws += h[j] * da[d + p1];
          vs += g[j] * da[d + p1];
          p1 -= step;
          if (p1 < 0)
          {
            p1 += length;
          }
        }
        l = level - i - 1;
        v[l * length + position] = vs;
        w[l * length + position] = ws;
        da = this.v;
        d = l * length;
      }
    }
    
    /// <summary>
    /// Получить точку
    /// </summary>
    /// <returns>Отфильтрованное значение</returns>
    /// <remarks>Производится реконструкция в обратном порядке.
    /// Коэф-ты каждого следующего уровня можно посчитать, зная только достаточное кол-во коэф-тов предыдущего, а именно 2^l. Это и вносит некоторую задержку.
    /// </remarks>
    public virtual float GetPoint()
    {
      int offset, l, step, i, p1, p2;
      float vs = 0;

      float[] da;
      int d;

      da = this.v;
      d = 0;

      offset = 0;

      for (l = 0; l < level; l++)
      {
        step = 1 << (level - l - 1);
        offset += step * (n - 1);
        vs = 0;
        p1 = position - offset;
        if (p1 < 0)
        {
          p1 += length;
        }

        p2 = p1;

        for (i = 0; i < n; i++)
        {
          vs += (w[l * length + p1] * h[i] + da[d+p1] * g[i]) / 2;
          p1 += step;
          if (p1 >= length)
          {
            p1 -= length;
          }
        }
        iv[l * length + p2] = vs;

        da = this.iv;
        d = l * length;
      }

      return vs;
    }

    /// <summary>
    /// возвращает задержку фильтра
    /// </summary>
    /// <returns></returns>
    public int GetLatency()
    {
      return length - 1;
    }

    #endregion
  }
}

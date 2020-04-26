namespace PskOnline.Math.Psa.Hrv
{
  /// <summary>
  /// This class implements statistics collection in the form of histogram.
  /// </summary>
  class HistogramStatCollector
  {
    #region private memebers
    private double[] bins;
    private int bins_count;
    private int total;
    private double xmin;
    private double xmax;
    #endregion

    #region public members
		/// Создать новый собиратель статистики.
		/** Собирается статистика в виде гистограммы из n столбцов, рассматривается часть сигнала, попадающая в промежуток [min_x, max_x] */
		public HistogramStatCollector(double min_x, double max_x, int n)
    {
      this.xmin = min_x;
      this.xmax = max_x;
      this.bins_count = n;
    	this.total = 0;
      this.bins = new double[this.bins_count];

      for (int i = n - 1; i >= 0; i--)
      {
        bins[i] = 0;
      }
    }

    public HistogramStatCollector()
    {
    }
		
		/// Добавить новую точку
    public void AddPoint(double x, double weight)
    {
      if ((x < xmin) || (x > xmax))
      {
        return;
      }
      int i = (int)(this.bins_count * (x - xmin) / (xmax - xmin));
      if ((i < 0) || (i >= this.bins_count))
      {
        return;
      }
      bins[i] += weight;
      total++;
    }

    /// <summary>
    /// Получить собранную статистику.		
    /// Возвращаемые данные содержатся во внутреннем буфере, поэтому их нельзя изменять!
    /// </summary>
    /// <param name="x"></param>
    /// <param name="n"></param>
		public double[] GetStats()
    {
      return this.bins;
    }

    /// <summary>
    /// Получить копию собранной статистики -- этот массив можно изменять.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="n"></param>
    /// <returns></returns>
		public double[] GetStatsCopy()
    {
      double[] bins_copy = new double[this.bins.Length];
      this.bins.CopyTo(bins_copy, 0);
      return bins_copy;
    } 


		public void DumpToFile(string FileName)
    {
    }
    #endregion

  }
}

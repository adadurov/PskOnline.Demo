namespace PskOnline.Methods.Svmr.ObjectModel
{
  using PskOnline.Methods.ObjectModel.Attributes;

  public class TestErrors  
	{
		public TestErrors () : base()
		{			
		}
		
		public TestErrors (TestErrors src) 
		{
			NormalCount = src.NormalCount;
			MissedCount = src.MissedCount;
			PrematureCount = src.PrematureCount;
		  LogicErrorCount = src.LogicErrorCount;
			TotalCount = src.TotalCount;
      FilteredPrematureCount = src.FilteredPrematureCount;
      FilteredMissedCount = src.FilteredMissedCount;
		}

	  /// <summary>
	  /// Количество реакций, которые при обработке были помечены как преждевременные
	  /// Это значит, что стимул уже загорелся, но время реакции оказалось меньше порога (обычно 150 мс)
	  /// </summary>
	  [ScriptComment("Количество \"нормальных\", отфильтрованных как преждевременные", "", "")]
    [Exportable]
    public int FilteredPrematureCount;

    /// <summary>
    /// Количество реакций, которые при обработке были помечены как преждевременные
    /// Это значит, что стимул загорелся и был потушен, но время реакции оказалось больше порога (обычно 1 с)
    /// </summary>
    [ScriptComment("Количество \"нормальных\" реакций, отфильтрованных как пропущенные", "", "")]
    [Exportable]
    public int FilteredMissedCount;

    /// <summary>
		/// Норма
		/// </summary>
    [ScriptComment("Количество \"правильных\" реакций", "", "")]
    [Exportable]
    public int NormalCount;

		/// <summary>
		/// Количество \"пропущенных\" стимулов (запаздывающих реакций)
		/// </summary>
    [ScriptComment("Количество \"пропущенных\" стимулов (запаздывающих реакций)", "", "")]
    [Exportable]
    public int MissedCount;

    /// <summary>
		/// Количество \"преждевременных\" (ложных) реакций
		/// </summary>
    [ScriptComment("Количество \"преждевременных\" (ложных) реакций", "", "")]
    [Exportable]
    public int PrematureCount;

    /// <summary>
    /// Эти реакции учитываются при формировании ИПН1
    /// </summary>
    [ScriptComment("Количество \"неправильных\" реакций", "", "")]
    [Exportable]
    public int WrongCount => PrematureCount + MissedCount;

    /// <summary>
    /// Количество иных ошибок.
    /// На текущий момент (2017.07.23) эти ошибки не учитываются при расчете ИПН1.
    /// Потому что никто не знает, что это такое.
    /// </summary>
    [Exportable]
    public int LogicErrorCount;
    
		/// <summary>
		/// Всего "зачетных" стимелов
		/// </summary>
    [ScriptComment("Общее количество \"зачетных\" стимулов", "", "")]
    [Exportable]
    public int TotalCount;
	}
}

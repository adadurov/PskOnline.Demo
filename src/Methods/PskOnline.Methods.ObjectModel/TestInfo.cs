namespace PskOnline.Methods.ObjectModel
{
  using System;
  using System.Linq;
  using PskOnline.Methods.ObjectModel.Attributes;
  using PskOnline.Methods.ObjectModel.Extensions;

  /// <summary>
  /// Класс для хранения общей информации об одной сессии
  /// тестирования по одной методике
  /// </summary>
  public class TestInfo
	{
	  public TestInfo()
	  {
	  }

    public TestInfo(TestInfo src)
    {
      Patient = new Patient(src.Patient);
      TestId = src.TestId;
      MethodId = src.MethodId;
      MethodModuleVersion = src.MethodModuleVersion;
      MethodAlias = src.MethodAlias;
      MethodTitle = src.MethodTitle;
      StartTime = src.StartTime;
      FinishTime = src.FinishTime;
      InspeсtionType = src.InspeсtionType;
      InspectionPlace = src.InspectionPlace;
      Comment = src.Comment;
      MachineName = src.MachineName;
      DatabaseName = src.DatabaseName;
    }

	  /// <summary>
	  /// An ID of the test session
	  /// </summary>
	  [Exportable(100)]
	  public Guid TestId { get; set; }

    /// <summary>
    /// Методика
    /// </summary>
    [ScriptComment("Идентификатор методики", "", "")]
    [Exportable(100)]
    public string MethodId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ScriptComment("Версия модуля, реализующего методику", "", "")]
    public string MethodModuleVersion { get; set; }

    /// <summary>
    /// Краткое название методики
    /// </summary>
    [ScriptComment("Краткое название методики", "", "")]
    [Exportable(80)]
    public string MethodAlias { get; set; }

    /// <summary>
    /// Краткое название методики
    /// </summary>
    [ScriptComment("Полное название методики", "", "")]
    public string MethodTitle { get; set; }

    /// <summary>
    /// Для удобного и единообразного получения информации о "дате и времени обследования"
    /// по договоренности этой датой считается момент окончания обследования.
    /// </summary>
    [Exportable(90)]
    public DateTimeOffset InspectionTime => FinishTime;

	  /// <summary>
    /// Время начала обследования
    /// </summary>
    [ScriptComment("Время начала обследования", "", "")]
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Время окончания обследования
    /// </summary>
    [ScriptComment("Время окончания обследования", "", "")]
    public DateTimeOffset FinishTime { get; set; }

    /// <summary>
    /// Тип обследования
    /// </summary>
    [Exportable(70)]
    public InspectionType InspeсtionType { get; set; }

    /// <summary>
    /// Место обследования
    /// </summary>
    [Exportable]
    public InspectionPlace InspectionPlace { get; set; }

	  [Exportable]
	  public string MachineName { get; set; }

	  [Exportable]
	  public string DatabaseName { get; set; }

    /// <summary>
    /// Комментарий к тесту (может описывать состояние обследуемого,
    /// причину, дополнительные условия обследования и т.п.)
    /// </summary>
    [Exportable]
    public string Comment;

    /// <summary>
    /// Обследуемые
    /// </summary>
    [ScriptComment("Обследуемый", "", "")]
    [Exportable(1000)]
    public Patient Patient;
  }
}

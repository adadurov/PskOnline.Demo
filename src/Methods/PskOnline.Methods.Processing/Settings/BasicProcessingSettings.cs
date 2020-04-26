namespace PskOnline.Methods.Processing.Settings
{
  using System;

  using PskOnline.Methods.ObjectModel.Settings;

  /// <summary>
  /// Базовые настройки обработки данных.
  /// </summary>
  public abstract class BasicProcessingSettings : BasicSettings
	{
	  protected BasicProcessingSettings()
		{
		}

	  protected BasicProcessingSettings(string methodId) : base(Category.Processing, methodId)
		{
		}

    /// <summary>
    /// Копирует настройки из объекта source
    /// </summary>
    /// <param name="source"></param>
    public override void CopyFrom(IMethodSettings source)
    {
      base.CopyFrom(source);
      if( source is BasicProcessingSettings src )
      {
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override object Clone()
    {
      throw new NotSupportedException("Cannot clone abstract class's object!");
    }

    public override void Default()
    {
    }
	}
}

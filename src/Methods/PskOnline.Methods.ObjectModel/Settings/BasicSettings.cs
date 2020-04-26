namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;
  using System.Diagnostics;

  /// <summary>
  /// Пустышка-базовый класс.
  /// Можно использовать, а можно нет.
  /// </summary>
  public abstract class BasicSettings : IMethodSettings
	{
    /// <summary>
    /// Должно быть public, чтобы была возможной XML-сериализация!!!
    /// </summary>
		public Category	m_category = Category.None;

    /// <summary>
    /// Должно быть public, чтобы была возможной XML-сериализация!!!
    /// </summary>
    public string		m_method_id = null;		

		public BasicSettings()
		{
		}

		public BasicSettings(BasicSettings src)
		{
      CopyFrom(src);
		}

    /// <summary>
    /// Копирует настройки из объекта source
    /// </summary>
    /// <param name="source"></param>
    public virtual void CopyFrom(IMethodSettings source)
    {
      BasicSettings src = source as BasicSettings;
      if( null != src )
      {
        if( string.IsNullOrEmpty(src.m_method_id) )
        {
          throw new ArgumentException("Source object being copied is invalid: m_method_id value is missing or invalid (empty string)", "src");
        }

        m_category = src.m_category;
        m_method_id = src.m_method_id;
      }
    }


		public BasicSettings(Category cat, string method_id)
		{
			Debug.Assert((method_id != null) && (method_id != string.Empty));

			m_category = cat;
			m_method_id = method_id;
		}

		protected string MethodId {get{return m_method_id;}}

		#region ISettings Members

		public abstract void Default();

		public virtual string GetMethodId()
		{
			return m_method_id;
		}

		public virtual Category GetCategory()
		{
			return m_category;
		}

		#endregion

		#region ICloneable Members

		public abstract object Clone();

		#endregion

    #region IComparable Members

    public virtual int CompareTo(object obj)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}

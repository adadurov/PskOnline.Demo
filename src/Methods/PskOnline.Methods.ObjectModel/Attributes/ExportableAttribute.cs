using System;

namespace PskOnline.Methods.ObjectModel.Attributes
{
    /// <summary>
    /// Указывает, что поля или свойства, помеченные данным атрибутом участвуют в экспорте данных.
    /// </summary>
    /// <remarks>
    /// 
    /// Члены (поля и свойства) классов (типов), помеченные этим атрибутом, должны содержать непустые 
    /// (non-null) значения, так как они используются не только конкретно при экспорте данных,
    /// но и при получении информации о формате экспортируемых данных.
    /// 
    /// Если поле или свойство является массивом нужно быть осторожным при добавлении в него
    /// элементов, имеющих тип, унаследованный от объявленного типа элемента массива.
    /// 
    /// Если поле или элемент является массивом, он должен быть непустым, т.е.
    /// иметь размер по крайней мере 1 по каждому измерению.
    /// 
    /// Если поле или элемент является массивом, реализация по умолчанию экспортирует только первый
    /// элемент массива (имеет индексы 0 по всем измерениям).
    /// </remarks>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
  public class ExportableAttribute : Attribute
  {
    public ExportableAttribute() : base() 
    {
    }

    public ExportableAttribute(int order) : base()
    {
      Order = order;
    }

    /// <summary>
    /// Позволяет добавить префиксы к найденным в ресурсах именам.
    /// Используется в случае, если экспортируемый объект содержит несколько полей одного типа.
    /// </summary>
    public ExportableAttribute(string titlePrefix, string namePrefix)
      : this(int.MinValue, titlePrefix, namePrefix, "")
    {
    }

    /// <summary>
    /// Позволяет добавить префиксы к найденным в ресурсах именам.
    /// Используется в случае, если экспортируемый объект содержит несколько полей одного типа.
    /// </summary>
    public ExportableAttribute(int order, string titlePrefix, string namePrefix)
     : this(order, titlePrefix, namePrefix, "")
    {
    }

    public ExportableAttribute(int order, string titlePrefix, string namePrefix, string titlePostfix)
    {
      TitlePrefix = titlePrefix;
      NamePrefix = namePrefix;
      TitlePostfix = titlePostfix;
      Order = order;
    }


    /// <summary>
    /// Позволяет добавить префиксы и постфиксы к найденным в ресурсах именам полей.
    /// Может быть полезно, если в одном объекте имеется несколько полей одного типа,
    /// или нужно добавить нечто специфическое к описанию полей объекта широкоиспользуемого класса.
    /// Постфикс добавляется без разделителя!!!
    /// </summary>
    public ExportableAttribute(string titlePrefix, string namePrefix, string titlePostfix)
      : base() 
    {
      TitlePrefix = titlePrefix;
      NamePrefix = namePrefix;
      TitlePostfix = titlePostfix;
      Order = int.MinValue;
    }

    public string TitlePostfix { get; private set; }
    
    public string TitlePrefix { get; private set; }

    public string NamePrefix { get; private set; }

    /// <summary>
    /// The columns containing higher-orders items
    /// will appear closer to the left/top side of the export output
    /// </summary>
    public int Order { get; private set; }
  }
}

namespace PskOnline.Methods.ObjectModel.Attributes
{
  using System;
  using System.Globalization;
  using System.Reflection;
  using System.Resources;
  using log4net;

  /// <summary>
  /// ������������� ����������� ������������ �������
  /// ����������� � ������ �������� � �������,
  /// �������������� � �������� � ��� �������� ������ ��������.
  /// </summary>
  [AttributeUsage(AttributeTargets.All)]
  public class ScriptCommentAttribute : System.Attribute
  {
    /// <summary>
    /// ������� �������, ������� ��������� ������� �� ��������.
    /// </summary>
    /// <param name="defaultString">
    /// �������, ������� ������������ � �������� �����������,
    /// ���� ��������� ������ �� �������� �� �������.</param>
    /// <param name="resourceName">��� �������.</param>
    /// <param name="stringId">������������� ������� � �������.</param>
    public ScriptCommentAttribute(
      string defaultString,
      string resourceName,
      string stringId
      )
    {
      _defaultString = defaultString;
      _resourceName = resourceName;
      _stringId = stringId;
    }

    private readonly string _resourceName;
    private readonly string _stringId;
    private readonly string _defaultString;

    /// <summary>
    /// returns comment value, trying to load it from assembly resources.
    /// resources name and string id is passed during construction,
    /// assembly is obtained from the item, that this attribute instance is used on
    /// </summary>
    public string GetComment(Assembly assembly)
    {
      try
      {
        if (string.IsNullOrEmpty(_resourceName) || string.IsNullOrEmpty(_stringId))
        {
          return _defaultString;
        }
        return LoadCommentFromResource(assembly);
      }
      catch (Exception ex)
      {
        var log = LogManager.GetLogger(typeof(ScriptCommentAttribute));
        var nl = Environment.NewLine;
        var assemblyName = assembly.FullName;
        log.Error(
          $"Unable to load script comment{nl}" +
          $"resource: {_resourceName}{nl}string id: {_stringId}{nl}assembly: {assemblyName}",
          ex
        );
      }
      return _defaultString;
    }

    internal string LoadCommentFromResource(System.Reflection.Assembly assembly)
    {
      var rm = new ResourceManager(_resourceName, assembly);
      return rm.GetString(_stringId);
    }
  }
}

using System;

namespace PskOnline.Methods.ObjectModel.Attributes
{
    /// <summary>
    /// ���������, ��� ���� ��� ��������, ���������� ������ ��������� ��������� � �������� ������.
    /// </summary>
    /// <remarks>
    /// 
    /// ����� (���� � ��������) ������� (�����), ���������� ���� ���������, ������ ��������� �������� 
    /// (non-null) ��������, ��� ��� ��� ������������ �� ������ ��������� ��� �������� ������,
    /// �� � ��� ��������� ���������� � ������� �������������� ������.
    /// 
    /// ���� ���� ��� �������� �������� �������� ����� ���� ���������� ��� ���������� � ����
    /// ���������, ������� ���, �������������� �� ������������ ���� �������� �������.
    /// 
    /// ���� ���� ��� ������� �������� ��������, �� ������ ���� ��������, �.�.
    /// ����� ������ �� ������� ���� 1 �� ������� ���������.
    /// 
    /// ���� ���� ��� ������� �������� ��������, ���������� �� ��������� ������������ ������ ������
    /// ������� ������� (����� ������� 0 �� ���� ����������).
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
    /// ��������� �������� �������� � ��������� � �������� ������.
    /// ������������ � ������, ���� �������������� ������ �������� ��������� ����� ������ ����.
    /// </summary>
    public ExportableAttribute(string titlePrefix, string namePrefix)
      : this(int.MinValue, titlePrefix, namePrefix, "")
    {
    }

    /// <summary>
    /// ��������� �������� �������� � ��������� � �������� ������.
    /// ������������ � ������, ���� �������������� ������ �������� ��������� ����� ������ ����.
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
    /// ��������� �������� �������� � ��������� � ��������� � �������� ������ �����.
    /// ����� ���� �������, ���� � ����� ������� ������� ��������� ����� ������ ����,
    /// ��� ����� �������� ����� ������������� � �������� ����� ������� ������������������� ������.
    /// �������� ����������� ��� �����������!!!
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

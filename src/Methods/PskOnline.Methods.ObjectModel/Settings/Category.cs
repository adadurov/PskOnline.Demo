namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;

	/// <summary>
  /// ������������ ��������� ��������� �������� ��� �������.
  /// �������� ������ ���� �������� �������.
  /// </summary>
  [Flags]
	public enum Category : int
	{
    /// <summary>
    /// ����������
    /// </summary>
		None = 0x0,

    /// <summary>
    /// "������������"
    /// </summary>
    Test = 0x1,

    /// <summary>
    /// "����������" -- ��������� ����������
    /// </summary>
    Device = 0x2,

    /// <summary>
    /// "�������������� ��������"
    /// </summary>
    ExtraLoad = 0x4,

    /// <summary>
    /// "���������"
    /// </summary>
    Processing = 0x8,

    /// <summary>
    /// "�������������" (�������� ������������� ������)
    /// </summary>
    Presentation = 0x10,

    /// <summary>
    /// "�������������" (�������� ������������� ������)
    /// </summary>
    Report = 0x20
	}

	/// <summary>
	/// ��������� ����� ��� �������������� ��������� � ������,
	/// ������� ����� �������������� � UI.
	/// FIXME: move the converter to a more appropriate place
	/// </summary>
	public static class CategoryConverter
	{
		public static string ToString(Category cat)
		{
      return category.ResourceManager.GetString(cat.ToString());
		}
	}

}
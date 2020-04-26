namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;
  using System.Collections.Generic;

	/// <summary>
  /// ���������-��������� �������� ��� ��������.
  /// �������� ��������� ���� ���������:
  /// ��� ���������� ������������, ��� ��������� ������,
  /// ��� ����������� ����������� �� ������, ��� ��������� ������.
  /// � ������� ����� ���� ��������� ������ ���������.
  /// 
  /// ������������ ��� ���������� ��������, ��� ������ � ������
  /// </summary>
  public interface IMethodSettingsPack : ICloneable, IEnumerable<IMethodSettings>
	{
		/// <summary>
		/// ��������� ��������� � �����.
		/// </summary>
		/// <param name="settings">���������, ������� ������ ���� ���������.</param>
		void Add(IMethodSettings settings);

    /// <summary>
    /// ������� ��������� ��������� ���������.
    /// </summary>
    /// <param name="category"></param>
    void Remove(Category category);

		/// <summary>
		/// ���������� ��� ���������/��������� �������� ������ ���������.
		/// </summary>
		IMethodSettings this[Category category] {get;set;}

    /// <summary>
    /// ���������� ��������� ���������
    /// </summary>
    int Count { get; }
	}
}

namespace PskOnline.Methods.ObjectModel.Settings
{
  using System;

	/// <summary>
  /// ��������� �������, ��������� ��������� ��������� �������� �������, ���������� ��������� �������������.
  /// ������������� ������ � ���������� ������������ ��� �������������� ������������ ��������
  /// (������� �������� ����������, ������������ ��� �������������� �����������).
  /// 
  /// TODO: BUGBUG: Check if the statement below is actually true.
  /// ��� ��������� �������� �������� ������������ �������
  /// bool System.Object.Equals(System.Object obj).
  /// ��� ��������� ������������ ������������ �������� ==
  /// </summary>  
  public interface IMethodSettings : ICloneable
	{
		/// <summary>
		/// ���������� ��������� ��������.
		/// </summary>
		Category GetCategory();

		/// <summary>
		/// ���������� ��������, � ������� ��������� ���������.
		/// </summary>
		/// <returns></returns>
		string GetMethodId();

    /// <summary>
    /// ������������� ��������� �� ���������.
    /// </summary>
    void Default();

    /// <summary>
    /// �������� ��������� �� ������� src
    /// </summary>
    /// <param name="source"></param>
    void CopyFrom(IMethodSettings source);

	}
}

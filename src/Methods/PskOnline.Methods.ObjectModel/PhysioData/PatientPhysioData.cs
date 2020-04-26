namespace PskOnline.Methods.ObjectModel.PhysioData
{
  using System.Collections.Generic;

  /// <summary>
  /// ����� ��� �������� ������ ������ ���������������� ������,
  /// ���������� � ������ ������������.
  /// </summary>
  /// <remarks>
  /// ���� ����� �������� ������ ������, ������� ����������� � �����������.
  /// ���������������� ����� ��� ��������� �� �����
  /// ������ ����� ����� �������� "breaking change", ��������� �������������� ���������
  /// ��� ���������� �������� �������������.
  /// </remarks>
  public class PatientPhysioData
  {
    public string PatientId { get; set; }

    /// <summary>
    /// ��� ������ ���� �������
    /// channel_id:string -> channelData:ChannelData
    /// </summary>
    public Dictionary<string, ChannelData> Channels { get; set; }

    /// <summary>
    /// ������������� �������
    /// markerId:long -> marker:Marker
    /// </summary>
    public Dictionary<long, Marker> Markers { get; set; }

    public int MaxMarkerId { get; set; }

    /// <summary>
    /// �������� ����� ��� ��������������� ������,
    /// ���������� � ������� ������ _�����������_ ����������
    /// (������� ����� ������������ ��� ��������� ������ ��������� ���������� ���������).
    /// </summary>
    public PatientPhysioData()
    {
    }

  }
}
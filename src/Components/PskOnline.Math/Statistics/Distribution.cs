using System;

namespace PskOnline.Math.Statistics
{
  /// <summary>
  /// ������������� (�����������).
  /// 
  /// ������ �������� (channels[0]) �������� �����������
  /// ��������� �������� ������ ���������� ��������� �������
  /// � �������� [min, min + band_width), ������ �������� (channels[1]) --
  /// ����������� ��������� � �������� [min + band_width, min + 2 * band_width) � �.�.
  /// ��� �������, ��� ��� �������� ������ ���������� ��������� �������
  /// ��������� � ��������� [min, max].
  /// </summary>
  public class Distribution
	{
		public Distribution()
		{
		}

		public Distribution(Distribution src)
		{
			min = src.min;
			max = src.max;
      mode = src.mode;
      mode_amplitude = src.mode_amplitude;
			channel_count = src.channel_count;
      channel_width = src.channel_width;
			if( src.channels != null )
			{
				this.channels = new double[src.channels.GetLength(0)];
        src.channels.CopyTo(this.channels, 0);
			}
		}

    /// <summary>
    /// ������ �����������
    /// </summary>
		public double min = 0;

    /// <summary>
    /// ����� �����������
    /// </summary>
		public double max = 0;
    
    /// <summary>
    /// ���������� ���������� �����������
    /// </summary>
		public int	  channel_count = 0;

    /// <summary>
    /// ������ ��������� �����������
    /// </summary>
    public double channel_width = 0;

    /// <summary>
    /// "�������� ��������� ��������"
    /// </summary>
    public double mode = 0;

    /// <summary>
    /// ��������� �����������, ���������������
    /// "�������� ���������� ��������"
    /// </summary>
    public double mode_amplitude = 0;

    /// <summary>
    /// ������
    /// </summary>
		public double[] channels;
	}
}

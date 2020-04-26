namespace PskOnline.Methods.Svmr.ObjectModel
{
  using System;

  /// <summary>
  /// ������� ��� �������� ����.
  /// </summary>
  public class SvmrAttempt
	{
		public SvmrAttempt()
		{
		}

		public SvmrAttempt(SvmrAttempt src)
		{
		  PhSyncBegin = src.PhSyncBegin;
		  PhSyncEnd = src.PhSyncEnd;

		  this.DelaySeconds = src.DelaySeconds;
		  this.IsTraining = src.IsTraining;

		  this.ReactionTimeSeconds = src.ReactionTimeSeconds;

			this.ReactionError = src.ReactionError;

		  this.ReactionAcceptedTime = src.ReactionAcceptedTime;
		  this.StimulusOccurredTime = src.StimulusOccurredTime;
		  this.Key = src.Key;
		}

    /// <summary>
    /// ����� ������ ������� ��� ������������� ��������������� ��������
    /// </summary>
	  public long PhSyncBegin { get; set; }

    /// <summary>
    /// ����� ��������� ������� ��� ������������� ��������������� ��������
    /// </summary>
	  public long PhSyncEnd { get; set; }

    /// <summary>
    /// ��������� �������� ����� ������� �������, �������
    /// </summary>
    public double DelaySeconds;

	  /// <summary>
	  /// �������� �� ������ ������ "�������������" ��������.
	  /// </summary>
	  public bool IsTraining = false;

    /// <summary>
    /// ������, ������� ���� ������ �������������
    /// </summary>
	  public ActionKey Key;

	  public DateTimeOffset StimulusOccurredTime = DateTimeOffset.MinValue;

	  public DateTimeOffset ReactionAcceptedTime = DateTimeOffset.MinValue;

    /// <summary>
    /// ����� ������� ������������ � ��������.
    /// </summary>
    public float ReactionTimeSeconds { get; set; } = 0.0f;

    public ReactionError ReactionError { get; set; } = ReactionError.NoError;
	}
}

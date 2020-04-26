namespace PskOnline.Methods.Svmr.ObjectModel
{
  using System;

  /// <summary>
  /// Реакция для методики ПЗМР.
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
    /// Метка начала стимула для синхронизации физиологических сигналов
    /// </summary>
	  public long PhSyncBegin { get; set; }

    /// <summary>
    /// Метка окончания стимула для синхронизации физиологических сигналов
    /// </summary>
	  public long PhSyncEnd { get; set; }

    /// <summary>
    /// Случайная задержка перед выдачей стимула, секунды
    /// </summary>
    public double DelaySeconds;

	  /// <summary>
	  /// Является ли данный стимул "тренировочным" стимулом.
	  /// </summary>
	  public bool IsTraining = false;

    /// <summary>
    /// Кнопка, которая была нажата пользователем
    /// </summary>
	  public ActionKey Key;

	  public DateTimeOffset StimulusOccurredTime = DateTimeOffset.MinValue;

	  public DateTimeOffset ReactionAcceptedTime = DateTimeOffset.MinValue;

    /// <summary>
    /// Время реакции обследуемого в секундах.
    /// </summary>
    public float ReactionTimeSeconds { get; set; } = 0.0f;

    public ReactionError ReactionError { get; set; } = ReactionError.NoError;
	}
}

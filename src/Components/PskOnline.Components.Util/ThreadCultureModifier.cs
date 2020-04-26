namespace PskOnline.Components.Util
{
  using System;
  using System.Threading;
  using System.Globalization;

  /// <summary>
  /// may be used in 'using' block to temporarily modify
  /// certain aspects of current thread's culture, like this:
  /// </summary>
  public class ThreadCultureModifier : IDisposable
  {
    private readonly System.Globalization.CultureInfo _ciOld;
    private readonly System.Globalization.CultureInfo _uiCiOld;

    public static ThreadCultureModifier SetFloatingPointNumberDecimalSeparator(string separator)
    {
      var threadCultureModifier = MakeThreadCultureModifier(out var ci, out var ciUi);
      ci.NumberFormat.NumberDecimalSeparator = ciUi.NumberFormat.NumberDecimalSeparator = separator;
      threadCultureModifier.Init(ci, ciUi);
      return threadCultureModifier;
    }

    private static ThreadCultureModifier MakeThreadCultureModifier(out CultureInfo ci, out CultureInfo ui_ci)
    {
      var modifier = new ThreadCultureModifier();
      ci = (CultureInfo)modifier._ciOld.Clone();
      ui_ci = (CultureInfo)modifier._uiCiOld.Clone();
      return modifier;
    }

    private ThreadCultureModifier()
    {
      _ciOld = Thread.CurrentThread.CurrentCulture;
      _uiCiOld = Thread.CurrentThread.CurrentUICulture;
    }

    protected void Init(CultureInfo ci, CultureInfo ui_ci)
    {
      Thread.CurrentThread.CurrentCulture = ci;
      Thread.CurrentThread.CurrentUICulture = ui_ci;
    }

    public void Dispose()
    {
      if ((null == _ciOld) || (null == _uiCiOld))
      {
        throw new InvalidOperationException("ThreadCultureModifier was used without proper initialization");
      }
      Thread.CurrentThread.CurrentCulture = _ciOld;
      Thread.CurrentThread.CurrentUICulture = _uiCiOld;
    }
  }
}

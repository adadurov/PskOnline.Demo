namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public static class PsaStatusTextProvider
  {
    public static string StatusText(PsaStatus status)
    {
      if (PsaStatus.Fail == status)
      {
        return status_strings.Status_Fail;
      }
      else if (PsaStatus.Conditional_Pass == status)
      {
        return status_strings.Status_Conditional_Pass;
      }
      else if (PsaStatus.Pass == status)
      {
        return status_strings.Status_Pass;
      }
      throw new NotSupportedException("Unexpected status value!");
    }
  }

}

namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;

  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public static class FinalStatusTextProvider
  {
    public static string StatusText(PreShiftFinalConclusion conclusion)
    {
      return StatusText(conclusion.Status);
    }
    public static string StatusText(PsaStatus status)
    {
      if( PsaStatus.Fail == status )
      {
        return status_strings.Final_Status_Fail;
      }
      else if( PsaStatus.Conditional_Pass == status )
      {
        return status_strings.Final_Status_Partial;
      }
      else if( PsaStatus.Pass == status )
      {
        return status_strings.Final_Status_Pass;
      }
      throw new NotSupportedException("Unexpected status value!");
    }
  }

}

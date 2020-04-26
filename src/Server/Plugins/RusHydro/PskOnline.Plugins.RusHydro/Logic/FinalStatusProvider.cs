namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public static class FinalStatusProvider
  {
    public static PreShiftFinalConclusion GetFinalConclusion(PreShiftHrvConclusion hrvConclusion, PreShiftSvmrConclusion svmrConclusion)
    {
      PreShiftFinalConclusion finalConclusion = new PreShiftFinalConclusion();

      switch (svmrConclusion.Status)
      {
        case PsaStatus.Pass:  // IPN1 is high
          if (hrvConclusion.LSR < LSR_HrvFunctionalState.OnTheEdge_2)
            finalConclusion.Status = PsaStatus.Fail;
          else if (hrvConclusion.LSR == LSR_HrvFunctionalState.OnTheEdge_2)
            finalConclusion.Status = PsaStatus.Conditional_Pass;
          else
            finalConclusion.Status = PsaStatus.Pass;
          break;

        case PsaStatus.Conditional_Pass:  // IPN1 is medium
          if (hrvConclusion.LSR < LSR_HrvFunctionalState.OnTheEdge_2)
            finalConclusion.Status = PsaStatus.Fail;
          else
            finalConclusion.Status = PsaStatus.Conditional_Pass;
          break;

        case PsaStatus.Fail: // IPN1 is low -- total failure
          finalConclusion.Status = PsaStatus.Fail;
          break;

        default:
          finalConclusion.Status = PsaStatus.Fail;
          break;
      }

      finalConclusion.Text = FinalStatusTextProvider.StatusText(finalConclusion);
      return finalConclusion;
    }
  }
}

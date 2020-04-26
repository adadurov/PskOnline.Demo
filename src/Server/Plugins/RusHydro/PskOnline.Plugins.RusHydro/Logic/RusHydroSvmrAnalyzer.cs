namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;

  using log4net;

  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.Test;
  using PskOnline.Methods.Processing.Contracts;
  using PskOnline.Methods.Svmr.ObjectModel;
  using PskOnline.Methods.Svmr.Processing;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public class RusHydroSvmrAnalyzer
  {
    private readonly ILog _log = LogManager.GetLogger(typeof(RusHydroHrvAnalyzer));
 
    public void GetSvmrConclusion(TestRawData svmrData, Guid testId, out PreShiftSvmrConclusion svmrConclusion)
    {
      _log.Info("Getting SVMR conclusion");

      double statM = 0;
      double statSigma = 0;
      double IPN1 = 0;
      svmrConclusion = null;
      var errorMessage = string.Empty;

      try
      {
        ProcessSvmrData(svmrData, out var svmrOutData);

        IPN1 = svmrOutData.IPN1;
        statM = svmrOutData.SvmrStatistics.m;
        statSigma = svmrOutData.SvmrStatistics.sigma;
        svmrConclusion = new PreShiftSvmrConclusion();
        if (svmrOutData.IPN1 >= 85)
        {
          svmrConclusion.Status = PsaStatus.Pass;
        }
        else if (svmrOutData.IPN1 >= 60)
        {
          svmrConclusion.Status = PsaStatus.Conditional_Pass;
        }
        else
        {
          svmrConclusion.Status = PsaStatus.Fail;
        }
      }
      catch (DataProcessingException ex)
      {
        _log.Error(ex);
        if (null == svmrConclusion)
        {
          svmrConclusion = new PreShiftSvmrConclusion();
        }
        svmrConclusion.Status = PsaStatus.Fail;
        IPN1 = 0;
        errorMessage = strings.SvmrTooManyErrors;
      }

      svmrConclusion.Text = PsaStatusTextProvider.StatusText(svmrConclusion.Status);
      svmrConclusion.TestId = testId;
      svmrConclusion.IPN1 = IPN1;
      svmrConclusion.MeanResponseTimeMSec = statM;
      svmrConclusion.Comment = $"ИПН1={IPN1:0.0}% MeanR={statM:#} SigmaR={statSigma:#} {errorMessage}";
      _log.Info($"SVMR conclusion IPN1 => {IPN1:F3}");
    }

    private void ProcessSvmrData(TestRawData svmrData, out SvmrResults svmrOutData)
    {
      var svmrProcessor = new SvmrDataProcessor();
      IMethodProcessedData tempSvmrOutData = svmrProcessor.ProcessData(svmrData);
      svmrOutData = tempSvmrOutData as SvmrResults;
      if (null == svmrOutData)
      {
        throw new ArgumentException(
          $"SVMR procesor returned object of an unexpected type. Data type: " +
          $"{tempSvmrOutData.GetType()}, expected: {typeof(SvmrResults)}");
      }
    }
  }
}

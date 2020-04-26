namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;

  using log4net;

  using PskOnline.Methods.Hrv.ObjectModel;
  using PskOnline.Methods.Hrv.Processing.Logic;
  using PskOnline.Methods.ObjectModel.Method;
  using PskOnline.Methods.ObjectModel.Test;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public class RusHydroHrvAnalyzer
  {
    private readonly ILog _log = LogManager.GetLogger(typeof(RusHydroHrvAnalyzer));

    public void GetHrvConclusion(TestRawData hrvData, Guid testId, out PreShiftHrvConclusion conclusion)
    {
      _log.Info("Getting HRV conclusion");

      double statM = 0;
      double statSigma = 0;
      double IN = 0;
      conclusion = null;

      try
      {
        ProcessHrvData(hrvData, out var hrvOutData);

        Upft130HrvClassifier classifier = new Upft130HrvClassifier();
        conclusion = classifier.MakePreshiftConclusion(hrvOutData);

        var LSR = conclusion.LSR_Text;
        if (string.IsNullOrEmpty(LSR))
        {
          throw new Exception("LSR_Text must be non-empty.");
        }

        statM = hrvOutData.CRV_STAT.m;
        statSigma = hrvOutData.CRV_STAT.sigma;
        IN = hrvOutData.Indicators.IN;
      }
      catch (Exception ex)
      {
        _log.Error(ex);
        if (null == conclusion)
        {
          conclusion = new PreShiftHrvConclusion();
        }
        conclusion.Status = PsaStatus.Fail;
        conclusion.LSR_Text = strings.UnknownError;
        conclusion.VSR = -1;
        conclusion.StateMatrixRow = -1;
        conclusion.StateMatrixCol = -1;
        conclusion.LSR = LSR_HrvFunctionalState.Critical_0;
      }

      conclusion.TestId = testId;
      conclusion.Text = PsaStatusTextProvider.StatusText(conclusion.Status);
      //      conclusion.Color = PsaStatusColorProvider.StatusColor(conclusion.Status);
      conclusion.MeanHR = 60000.0 / statM;
      conclusion.IN = IN;
      conclusion.Comment =
        $"LSR={conclusion.LSR_Text}, VSR={conclusion.VSR:0.000}, MRR={statM:#}, " +
        $"SRR={statSigma:#}, SMLoc=({conclusion.StateMatrixRow};{conclusion.StateMatrixCol})";
    }

    private void ProcessHrvData(TestRawData hrvData, out HrvResults hrvOutData)
    {
      var hrvProcessor = new HrvDataProcessor_Pro();

      IMethodProcessedData tempHrvOutData = hrvProcessor.ProcessData(hrvData);
      hrvOutData = tempHrvOutData as HrvResults;
      if (null == hrvOutData)
      {
        throw new ArgumentException(
          $"HRV processor returned object of an unexpected type. Data type: " +
          $"{tempHrvOutData.GetType()}, expected: {typeof(HrvResults)}");
      }
    }

  }
}

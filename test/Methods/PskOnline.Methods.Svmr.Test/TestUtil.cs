namespace PskOnline.Methods.Svmr.Test
{
  static class TestUtil
  {
    internal static string[] TestFileNames
    {
      get
      {
        return new string[]
        {
          "2009-05-12_18.39_svmr.psk.json",
          "2009-11-06_13.30_svmr.psk.json"
        };
      }
    }

    internal static string AllStimuliSkipped
    {
      get
      {
        return "2017-07-24_23.37_svmr_all_stimuli_skipped.psk.json";
      }
    }

    internal static string AllReactionsPremature
    {
      get
      {
        return "2017-07-24_23.28_svmr_all_reactions_premature.psk.json";
      }
    }
  }
}

namespace PskOnline.Server.Plugins.RusHydro.Logic
{
  using System;

  using PskOnline.Server.Plugins.RusHydro.ObjectModel;

  public static class PsaStatusColorProvider
  {
    public static System.Drawing.Color StatusColor(PsaStatus status)
    {
      if (PsaStatus.Fail == status)
      {
        return System.Drawing.Color.Red;
      }
      else if (PsaStatus.Pass == status)
      {
        return System.Drawing.Color.FromArgb(0x00, 0xbb, 0x00);//also tried ForestGreen;
      }
      else if (PsaStatus.Conditional_Pass == status)
      {
        return System.Drawing.Color.Orange;
      }
      throw new NotSupportedException("Unexpected status value!");
    }
  }

}

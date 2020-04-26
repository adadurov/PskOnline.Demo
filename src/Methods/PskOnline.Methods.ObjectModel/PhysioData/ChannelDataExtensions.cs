namespace PskOnline.Methods.ObjectModel.PhysioData
{
  using System.Linq;

  public static class ChannelDataExtensions
  {

    public static float[] GetDataAsFloat(this ChannelData _this)
    {
      return _this.Data.Select( sample => (float)sample ).ToArray();
    }
  }
}

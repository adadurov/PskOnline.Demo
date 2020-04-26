namespace PskOnline.Client.Api
{
  using System.Net;
  using System.Security;

  public static class SecureStringExtensions
  {
    public static SecureString ToSecureString(this string aString)
    {
      var ss = new SecureString();
      foreach (char c in aString)
      {
        ss.AppendChar(c);
      }
      return ss;
    }

    public static string ToInsecureString(this SecureString secureString)
    {
      return new NetworkCredential(string.Empty, secureString).Password;
    }
  }
}

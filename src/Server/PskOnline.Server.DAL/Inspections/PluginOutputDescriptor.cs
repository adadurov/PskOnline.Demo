using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Server.DAL.Inspections
{
  public class PluginOutputDescriptor
  {
    /// <summary>
    /// A mnemonic plugin ID (e.g. pskonline-demo)
    /// </summary>
    public string PluginType { get; set; }

    /// <summary>
    /// A URL that may be used to fetch the results
    /// </summary>
    public string ResultsUrl { get; set; }
  }
}

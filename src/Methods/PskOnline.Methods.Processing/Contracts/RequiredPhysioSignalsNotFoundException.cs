using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Processing.Contracts
{

  public class RequiredPhysioSignalsNotFoundException : DataProcessingException
  {
    public RequiredPhysioSignalsNotFoundException(string[] not_found_channels)
      : base(string.Empty)
    {
      this.m_not_found_channels = not_found_channels;
    }
    private string[] m_not_found_channels;
  }

}

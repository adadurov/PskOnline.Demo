using System;
using System.Collections.Generic;
using System.Text;
using PskOnline.Methods.ObjectModel.Test;

namespace PskOnline.Methods.Svmr.ObjectModel
{
  public class SvmrRawData : TestRawData
  {
    public List<SvmrAttempt> Attempts { get; set; }
  }
}

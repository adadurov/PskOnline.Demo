using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Hrv.Processing.Logic
{
  /// <summary>
  /// stub class to test core functionality of basic data processor that is declared abstract
  /// </summary>
  class HrvDataProcessorStub : HrvBasicDataProcessor
  {
    protected override CrvTwoDimConclusionDatabase GetTwoDimConslusionDatabase()
    {
      throw new NotImplementedException();
    }
  }
}

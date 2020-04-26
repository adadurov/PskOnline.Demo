namespace PskOnline.Methods.Svmr.Test
{
  using System.IO;

  using NUnit.Framework;

  using PskOnline.Components.Log;
  using PskOnline.Components.Util;
  using PskOnline.Methods.Svmr.Processing;

  [TestFixture]
  public class BuiltinScreenPresentation_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(BuiltinScreenPresentation_UTest));

    [SetUp]
    public void SetUp()
    {
      LogHelper.ConfigureConsoleLogger();
    }

    [TearDown]
    public void TearDown()
    {
      LogHelper.ShutdownLogSystem();
    }

    [Test]
    [Explicit]
    [Category("Interactive")]
    public void Test_Presentation_0_Interactive()
    {
      // this test implies GUI interaction with the user
      Test_Processing_Internal(TestUtil.TestFileNames[0], -1, -1, 0);
    }

    [Test]
    public void Test_Presentation_0()
    {
      Test_Processing_Internal(TestUtil.TestFileNames[0], -1, -1, 200);
    }

    [Test]
    public void Test_Processing_All_Stimuli_Skipped_Shall_Not_Throw()
    {
      Test_Processing_Internal(TestUtil.AllStimuliSkipped, -1, -1, 200);
    }

    [Test]
    public void Test_Processing_All_Reactions_Premature_Shall_Not_Throw()
    {
      Test_Processing_Internal(TestUtil.AllReactionsPremature, -1, -1, 200);
    }

    [Test]
    [Explicit]
    [Category("Interactive")] 
    public void Test_Presentation_1_Interactive()
    {
      // this test implies GUI interaction with the user
      Test_Processing_Internal(TestUtil.TestFileNames[1], -1, -1, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="testDataFileName"></param>
    /// <param name="stateMatrixRow"></param>
    /// <param name="stateMatrixCol"></param>
    /// <param name="timeout">
    /// Если больше нуля то это время в миллисекундах, в течение которого форма отображается на экране.
    /// Если меньше или равен нулю -- форма отображается на экране, пока не будет закрыта (оператором).
    /// </param>
    void Test_Processing_Internal(string testDataFileName, int stateMatrixRow, int stateMatrixCol, int timeout)
    {
      var currentlyOpenDataProcessor = new SvmrDataProcessor();

      string full_filename = FileHelpers.GetPathFromExecutingAssembly(
        Path.Combine("unit_test_data", testDataFileName));

      var settings = new ProcessingSettings();

      SvmrFileProcessingHelper.ProcessFileWithSettings(full_filename, settings);

      if ((-1 != stateMatrixRow) && (-1 != stateMatrixCol))
      {
        currentlyOpenDataProcessor.UTest_SetStateMatrixState(stateMatrixRow, stateMatrixCol);
      }
    }
  }
}

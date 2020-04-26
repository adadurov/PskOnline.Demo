namespace PskOnline.Methods.Hrv.ObjectModel
{
  public class CardioInterval
  {
    /// <summary>
    /// default constructor for serialization
    /// </summary>
    public CardioInterval() { }

    public CardioInterval(double beginMSec, double endMSec, int beginContrIndex, int endContrIndex)
    {
      m_BeginMark = beginMSec;
      m_EndMark = endMSec;

      m_Begin_HC_Number = beginContrIndex;
      m_End_HC_Number = endContrIndex;
    }

    private double m_BeginMark;
    private double m_EndMark;

    internal int m_Begin_HC_Number;
    internal int m_End_HC_Number;

    /// <summary>
    /// моментом возникновения интервала считается момент его окончания
    /// </summary>
    public double moment
    {
      get
      {
        return m_EndMark;
      }
    }

    public double duration
    {
      get
      {
        return m_EndMark - m_BeginMark;
      }
    }
  }
}

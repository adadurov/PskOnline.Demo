namespace PskOnline.Methods.Hrv.ObjectModel
{
  using System;

  /// <summary>
  /// Defines 'personal' HRV norm.
  /// 
  /// </summary>
  /// <remarks>
  /// This is a serializable class!
  /// </remarks>
  public class PersonalHrvNorm
  {
    /// <summary>
    /// this constructor is for de-serialization only
    /// </summary>
    public PersonalHrvNorm()
    {
    }

    protected PersonalHrvNorm(double normalHeartRate)
    {
      NormalHeartRateAtRestBpm = normalHeartRate;
    }

    public static PersonalHrvNorm GetInstance(double normalHeartRate)
    {
      return new PersonalHrvNorm(normalHeartRate);
    }

    public PersonalHrvNorm(PersonalHrvNorm src)
    {
      if (src != null)
      {
        NormalHeartRateAtRestBpm = src.NormalHeartRateAtRestBpm;
      }
    }

    /// <summary>
    /// ѕульс в покое (единиц в минуту)
    /// </summary>
    public double NormalHeartRateAtRestBpm { get; set; }

    /// <summary>
    /// Compares values of the norms
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool operator ==(PersonalHrvNorm a, PersonalHrvNorm b)
    {
      if (object.ReferenceEquals(a, b))
      {
        return true;
      }
      if (null == a)
      {
        return b.Equals(a);
      }
      else
      {
        return a.Equals(b);
      }
    }

    public static bool operator !=(PersonalHrvNorm a, PersonalHrvNorm b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Compares values of the norms
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      if( obj is PersonalHrvNorm temp )
      {
        return NormalHeartRateAtRestBpm == temp.NormalHeartRateAtRestBpm;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return NormalHeartRateAtRestBpm.GetHashCode();
    }

  }
}

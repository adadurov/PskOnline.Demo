namespace PskOnline.Methods.ObjectModel.Extensions
{
  using System;

  public static class PatientExtension
  {
    /// <summary>
    /// Возвращает возраст в полных годах на указанную дату (день).
    /// Если указанная дата раньше даты рождения обследуемого
    /// </summary>
    /// <param name="moment"></param>
    /// <returns></returns>
    public static int GetAgeAtGivenMoment(this Patient _this, DateTimeOffset moment)
    {
      DateTime now = new DateTime(moment.Year, moment.Month, moment.Day, 12, 0, 0);
      DateTime born = new DateTime(_this.BirthDate.Year, _this.BirthDate.Month, _this.BirthDate.Day, 12, 0, 0);

      if (now < born)
      {
        // patient not yet born
        return -1;
      }

      if (now.DayOfYear < born.DayOfYear)
      {
        return now.Year - born.Year - 1;
      }
      else
      {
        return now.Year - born.Year;
      }
    }
  }
}

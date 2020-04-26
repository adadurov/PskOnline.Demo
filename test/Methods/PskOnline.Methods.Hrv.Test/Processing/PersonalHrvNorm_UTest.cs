namespace PskOnline.Methods.Hrv.Test.Processing
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using NUnit.Framework;
  using PskOnline.Components.Log;
  using PskOnline.Methods.Hrv.ObjectModel;

  [TestFixture]
  public class PersonalHrvNorm_UTest
  {
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(PersonalHrvNorm_UTest));

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
    public void TestCopyConstructor_copy_null_no_exceptions()
    {
      var anotherNorm = new PersonalHrvNorm(null);
    }

    [Test]
    public void TestCopyConstructor_copy_non_null()
    {
      var aNorm1 = new PersonalHrvNorm(null);
      aNorm1.NormalHeartRateAtRestBpm = 95;
      aNorm1.NormalHeartRateAtRestBpm = 70;

      var aNorm2 = new PersonalHrvNorm(aNorm1);
      Assert.IsTrue(aNorm1.Equals(aNorm2));
      Assert.IsTrue(aNorm2.Equals(aNorm1));
      Assert.IsTrue(aNorm1 == aNorm2);
      Assert.IsTrue(aNorm2 == aNorm1);
      Assert.IsFalse(aNorm1 != aNorm2);
      Assert.IsFalse(aNorm2 != aNorm1);
    }

    [Test]
    public void TestEqualityOperator_non_equal()
    {
      var aNorm1 = new PersonalHrvNorm();
      aNorm1.NormalHeartRateAtRestBpm = 95;
      var aNorm2 = new PersonalHrvNorm();
      aNorm2.NormalHeartRateAtRestBpm = 100;

      Assert.IsFalse(aNorm1.Equals(aNorm2));
      Assert.IsFalse(aNorm2.Equals(aNorm1));
      Assert.IsFalse(aNorm1 == aNorm2);
      Assert.IsFalse(aNorm2 == aNorm1);
      Assert.IsTrue(aNorm1 != aNorm2);
      Assert.IsTrue(aNorm2 != aNorm1);
    }

    [Test]
    public void TestEqualityOperator_equal()
    {
      var aNorm1 = new PersonalHrvNorm();
      aNorm1.NormalHeartRateAtRestBpm = 95;
      aNorm1.NormalHeartRateAtRestBpm = 70;

      var aNorm2 = new PersonalHrvNorm();
      aNorm2.NormalHeartRateAtRestBpm = 105;
      aNorm2.NormalHeartRateAtRestBpm = 70;

      Assert.IsTrue(aNorm1.Equals(aNorm2));
      Assert.IsTrue(aNorm2.Equals(aNorm1));
      Assert.IsTrue(aNorm1 == aNorm2);
      Assert.IsTrue(aNorm2 == aNorm1);
      Assert.IsFalse(aNorm1 != aNorm2);
      Assert.IsFalse(aNorm2 != aNorm1);

    }

    [Test]
    public void TestEqualityOperator_same_instance()
    {
      PersonalHrvNorm aNorm = new PersonalHrvNorm();
      Assert.IsTrue(aNorm.Equals(aNorm));
#pragma warning disable CS1718 
      Assert.IsTrue(aNorm == aNorm);
      Assert.IsFalse(aNorm != aNorm);
#pragma warning restore CS1718
    }

    [Test]
    public void TestEqualityOperator_left_null()
    {
      PersonalHrvNorm aNorm = new PersonalHrvNorm();
      Assert.IsFalse(null == aNorm);
      Assert.IsTrue(null != aNorm);
    }

    [Test]
    public void TestEqualityOperator_right_null()
    {
      PersonalHrvNorm aNorm = new PersonalHrvNorm();
      Assert.IsFalse(aNorm == null);
      Assert.IsTrue(aNorm != null);
      Assert.IsFalse(aNorm.Equals(null));
    }

  }
}

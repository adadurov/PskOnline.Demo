namespace PskOnline.Service.Test
{
  using System;
  using System.Threading.Tasks;
  using NUnit.Framework;

  public static class AsyncAssert
  {
    /// <summary>
    /// Assert that an async method fails due to a specific exception.
    /// </summary>
    /// <typeparam name="T">Exception type expected</typeparam>
    /// <param name="asyncDelegate">Test async delegate</param>
    public static async Task<T> ThrowsAsync<T>(Func<Task> asyncDelegate) where T : Exception
    {
      T caughtException = null;

      try
      {
        await asyncDelegate();
        Assert.Fail("Expected exception of type: {0}", typeof(T));
      }
      catch (T expectedException)
      {
        caughtException = expectedException;
      }
      catch (AssertionException)
      {
        throw;
      }
      catch (Exception ex)
      {
        Assert.Fail("Expected exception of type: {0} but was: {1}", typeof(T), ex);
      }

      return caughtException;
    }
  }
}

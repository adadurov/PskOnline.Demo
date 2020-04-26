namespace PskOnline.Client.Api
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  public class ItemNotFoundException : Exception
  {
    public ItemNotFoundException(string message) : base(message)
    {
    }
  }
}

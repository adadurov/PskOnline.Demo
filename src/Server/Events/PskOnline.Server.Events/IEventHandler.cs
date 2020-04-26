using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Server.Shared.Contracts.Events
{
    public interface IEventHandler<TEvent>
    {
      void HandleEvent(TEvent eventItem);
    }
}

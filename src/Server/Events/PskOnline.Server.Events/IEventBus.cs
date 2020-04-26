using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Server.Shared.Contracts.Events
{
    public interface IEventBus<TEvent>
    {
      void Subscribe(IEventHandler<TEvent> handler);

      void Publish(TEvent eventItem);
    }
}

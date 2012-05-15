using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NancySelfHost.WebSockets
{
    public interface IWebSocketListener
    {
        void HandleConnected();
        void HandleDisconnected();
    }
}

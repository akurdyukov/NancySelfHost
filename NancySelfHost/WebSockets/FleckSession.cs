using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Fleck;

namespace NancySelfHost.WebSockets
{
    public class FleckSession : ISession
    {
        private readonly IWebSocketConnection connection;

        public FleckSession(IWebSocketConnection connection)
        {
            this.connection = connection;
        }

        public EndPoint ClientAddress
        {
            get { return new IPEndPoint(IPAddress.Parse(connection.ConnectionInfo.ClientIpAddress), 0); }
        }

        public void Send(string message)
        {
            connection.Send(message);
        }
    }
}

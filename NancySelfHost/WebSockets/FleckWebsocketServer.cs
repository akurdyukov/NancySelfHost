using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Fleck;
using NancySelfHost.Auth;

namespace NancySelfHost.WebSockets
{
    public class FleckWebSocketServer : AbstractWebSocketServer
    {
        private readonly WebSocketServer server;
        public FleckWebSocketServer(int port, string hostName, TimeSpan timeout, IUserDatabase userDatabase) : base(userDatabase)
        {
            FleckLog.Level = LogLevel.Debug;
            server = new WebSocketServer(string.Format("ws://{0}:{1}", hostName, port));
        }

        public override void Start()
        {
            server.Start(socket =>
                             {
                                 socket.OnOpen = () => OnOpen(socket);
                                 socket.OnClose = () => OnClose(socket);
                                 socket.OnMessage = message => OnMessage(socket, message);
                             });
        }

        public override void Stop()
        {
            server.Dispose();
        }

        private void OnMessage(IWebSocketConnection conn, string message)
        {
            Console.WriteLine("Message " + message + " from " + conn.ConnectionInfo.Origin);
            ParseIncoming(new FleckSession(conn), message);
        }

        private void OnClose(IWebSocketConnection conn)
        {
            Console.WriteLine("Closing connection from " + conn.ConnectionInfo.Origin);
            LogoutUser(new FleckSession(conn));
        }

        private void OnOpen(IWebSocketConnection conn)
        {
            Console.WriteLine("Opening connection from " + conn.ConnectionInfo.Origin);
            RegisterUser(new FleckSession(conn));
        }
    }
}

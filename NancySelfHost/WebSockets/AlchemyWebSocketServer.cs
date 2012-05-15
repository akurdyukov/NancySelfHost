using System;
using System.Net;
using System.Threading;
using Alchemy;
using Alchemy.Classes;
using NancySelfHost.Auth;

namespace NancySelfHost.WebSockets
{
    public class AlchemyWebSocketServer : AbstractWebSocketServer
    {
        private readonly WebSocketServer server;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        public AlchemyWebSocketServer(int port, IPAddress ipAddress, TimeSpan timeout, IUserDatabase userDatabase) : base(userDatabase)
        {
            server = new WebSocketServer(port, ipAddress)
                         {
                             TimeOut = timeout,
                             FlashAccessPolicyEnabled = true,
                             OnConnected = OnConnected,
                             OnDisconnect = OnDisconnect,
                             OnReceive = OnReceive,
                             OnSend = OnSend
                         };
        }

        public override void Start()
        {
            server.Start();
        }

        public override void Stop()
        {
            tokenSource.Cancel();
            server.Stop();
        }

        private void OnConnected(UserContext context)
        {
            Console.WriteLine("Client Connection From : " + context.ClientAddress);
            RegisterUser(new AlchemySession(context));
        }

        private void OnReceive(UserContext context)
        {
            Console.WriteLine("Received Data From :" + context.ClientAddress);

            string jsonString = context.DataFrame.ToString();
            ParseIncoming(new AlchemySession(context), jsonString);
        }

        private void OnDisconnect(UserContext context)
        {
            Console.WriteLine("Client Disconnected : " + context.ClientAddress);
            LogoutUser(new AlchemySession(context));
        }

        private void OnSend(UserContext context)
        {
            Console.WriteLine("Data sent to : " + context.ClientAddress);
        }

    }
}
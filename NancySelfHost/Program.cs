using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
using NancySelfHost.Auth;
using NancySelfHost.WebSockets;
using NancySelfHost.WebSockets.Messages;

namespace NancySelfHost
{
    public class Program
    {
        private static IEnumerable<IPAddress> GetAllUnicastAddresses()
        {
            // This works on both Mono and .NET , but there is a difference: it also
            // includes the LocalLoopBack so we need to filter that one out
            List<IPAddress> addresses = new List<IPAddress>();
            // Obtain a reference to all network interfaces in the machine
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                addresses.AddRange(from IPAddressInformation uniCast in properties.UnicastAddresses 
                                   where uniCast.Address.AddressFamily != AddressFamily.InterNetworkV6 
                                   select uniCast.Address);
            }
            return addresses;
        }

        private static Uri[] GetBindableUris()
        {
            IEnumerable<IPAddress> addresses = GetAllUnicastAddresses();
            IList<Uri> urls = new List<Uri>();

            urls.Add(new Uri("http://localhost:8090/"));
            foreach (IPAddress address in addresses)
            {
                IPHostEntry entry = Dns.GetHostEntry(address);
                urls.Add(new Uri(string.Format("http://{0}:8090/", address)));
                urls.Add(new Uri(string.Format("http://{0}:8090/", entry.HostName)));
            }
            return urls.ToArray();
        }

        public static void Main(string[] args)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            var nancyHost = new NancyHost(GetBindableUris());
            IWebSocketServer wsServer = new FleckWebSocketServer(8091, "local.fvendor.com",
                                                                   new TimeSpan(0, 5, 0), new UserDatabase());
            wsServer.Register<AuthMessage>();

            Task nancyTask = new Task(() =>
                                          {
                                              nancyHost.Start();

                                              Console.WriteLine("Nancy now listening");

                                              while (!ct.IsCancellationRequested)
                                              {
                                                  Thread.Sleep(1000);
                                              }
                                              nancyHost.Stop();  // stop hosting
                                          }, tokenSource.Token);

            Task websocketTask = new Task(() =>
                                              {
                                                  wsServer.Start();

                                                  Console.WriteLine("Websockets listener started");

                                                  while (!ct.IsCancellationRequested)
                                                  {
                                                      Thread.Sleep(1000);
                                                  }
                                                  wsServer.Stop();  // stop hosting
                                              }, tokenSource.Token);

            Task logPusher = new Task(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    LogMessage message = new LogMessage
                    {
                        MessageText = "Another message",
                        When = DateTime.Now
                    };
                    wsServer.Broadcast(message);
                    Thread.Sleep(100);
                }
            }, tokenSource.Token);

            nancyTask.Start();
            websocketTask.Start();
            logPusher.Start();

            var line = Console.ReadLine();
            while (line != "quit")
            {
                line = Console.ReadLine();
            }
            tokenSource.Cancel();
        }
    }
}

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

            Task nancyTask = new Task(() =>
                                          {
                                              var nancyHost = new NancyHost(GetBindableUris());
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
                                                  var aServer = new WebSocketListener(8091, IPAddress.Any,
                                                                                      new TimeSpan(0, 5, 0), new UserDatabase());
                                                  aServer.Start();

                                                  Console.WriteLine("Websockets listener started");

                                                  while (!ct.IsCancellationRequested)
                                                  {
                                                      Thread.Sleep(1000);
                                                  }
                                                  aServer.Stop();  // stop hosting
                                              }, tokenSource.Token);

            nancyTask.Start();
            websocketTask.Start();

            var line = Console.ReadLine();
            while (line != "quit")
            {
                line = Console.ReadLine();
            }
            tokenSource.Cancel();
        }
    }
}

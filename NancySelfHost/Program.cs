using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Nancy.Hosting.Self;

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
            var nancyHost = new NancyHost(GetBindableUris());
            nancyHost.Start();

            Console.WriteLine("Nancy now listening");

            var line = Console.ReadLine();
            while (line != "quit")
            {
                line = Console.ReadLine();
            }
        }
    }
}

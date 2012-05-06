using System;
using Nancy.Hosting.Self;

namespace NancySelfHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var nancyHost = new NancyHost(new Uri("http://localhost:8081"));
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

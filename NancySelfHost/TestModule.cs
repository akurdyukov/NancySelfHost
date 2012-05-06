using System;
using Nancy;

namespace NancySelfHost
{
    public class TestModule : NancyModule
    {
        public TestModule()
        {
            Before += ctx =>
                          {
                              Console.WriteLine("Before...");
                              return null;
                          };

            Get["/"] = parameters =>
            {
                Console.WriteLine("Visit /");
                return View["index", Request.Url];
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Diagnostics;

namespace NancySelfHost
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get
            {
                return new DiagnosticsConfiguration { Password = @"123" };
            }
        }
    }
}

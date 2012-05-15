using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NancySelfHost.WebSockets
{
    public interface ISession
    {
        EndPoint ClientAddress { get; }
        void Send(string message);
    }
}

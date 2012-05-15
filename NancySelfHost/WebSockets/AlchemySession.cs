using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Alchemy.Classes;

namespace NancySelfHost.WebSockets
{
    public class AlchemySession : ISession
    {
        private readonly UserContext context;

        public AlchemySession(UserContext context)
        {
            this.context = context;
        }

        public EndPoint ClientAddress
        {
            get { return context.ClientAddress; }
        }

        public void Send(string message)
        {
            context.Send(message);
        }
    }
}

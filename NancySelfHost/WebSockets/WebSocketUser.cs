using System;
using Alchemy.Classes;

namespace NancySelfHost.WebSockets
{
    /// <summary>
    /// Holds the name and context instance for an online user
    /// </summary>
    public class WebSocketUser
    {
        public string Name = String.Empty;
        public ISession Session { get; set; }
    }
}
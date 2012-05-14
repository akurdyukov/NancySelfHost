using System;

namespace NancySelfHost.WebSockets.Messages
{
    [WebSocketMessage("Log")]
    public class LogMessage
    {
        public string MessageText { get; set; }
        public DateTime When { get; set; }
    }
}

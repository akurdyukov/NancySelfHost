using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NancySelfHost.WebSockets.Messages
{
    [WebSocketMessage("Log")]
    public class LogMessage
    {
        public string MessageText { get; set; }

        //[JsonConverter(typeof(JavaScriptDateTimeConverter))]
        public DateTime When { get; set; }
    }
}

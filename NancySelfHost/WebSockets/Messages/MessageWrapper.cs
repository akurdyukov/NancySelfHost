using Newtonsoft.Json;

namespace NancySelfHost.WebSockets.Messages
{
    public class MessageWrapper
    {
        [JsonProperty("Type")]
        public string TypeName { get; set; }
        [JsonProperty("Msg")]
        public object Message { get; set; }
    }
}
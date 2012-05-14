namespace NancySelfHost.WebSockets.Messages
{
    [WebSocketMessage("Auth")]
    public class AuthMessage
    {
        public string SessionId { get; set; }
    }
}

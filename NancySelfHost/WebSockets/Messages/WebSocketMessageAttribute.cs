using System;

namespace NancySelfHost.WebSockets.Messages
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WebSocketMessageAttribute : Attribute
    {
        public string TypeName { get; private set; }

        public WebSocketMessageAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}

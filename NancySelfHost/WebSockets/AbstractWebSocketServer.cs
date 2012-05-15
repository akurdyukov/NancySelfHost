using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NancySelfHost.Auth;
using NancySelfHost.WebSockets.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NancySelfHost.WebSockets
{
    public abstract class AbstractWebSocketServer : IWebSocketServer
    {
        private readonly IDictionary<string, Type> registeredTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Store the list of online users. Wish I had a ConcurrentList. 
        /// </summary>
        private readonly ConcurrentDictionary<WebSocketUser, User> onlineUsers = new ConcurrentDictionary<WebSocketUser, User>();

        private readonly IUserDatabase userDatabase;

        protected AbstractWebSocketServer(IUserDatabase userDatabase)
        {
            this.userDatabase = userDatabase;
        }

        protected string GetTypeName(Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeof(WebSocketMessageAttribute), false);
            if (attrs.Length < 1)
            {
                throw new NotImplementedException("Websocket message must have WebSocketMessage attribute");
            }
            WebSocketMessageAttribute attr = (WebSocketMessageAttribute)attrs[0];
            return attr.TypeName;
        }

        protected Type GetTypeByName(string name)
        {
            return registeredTypes[name];
        }

        public void Register<T>() where T : class
        {
            registeredTypes.Add(GetTypeName(typeof(T)), typeof(T));
        }

        public abstract void Start();
        public abstract void Stop();

        private MessageWrapper PackMessage(object message)
        {
            return new MessageWrapper {Message = message, TypeName = GetTypeName(message.GetType())};
        }

        public virtual void Send(object message, User user)
        {
            MessageWrapper wrapper = PackMessage(message);
            string json = JsonConvert.SerializeObject(wrapper);

            KeyValuePair<WebSocketUser, User> pair = onlineUsers.FirstOrDefault(p => p.Value == user);
            if (pair.Value == null)
            {
                Console.WriteLine("User " + user + " is not connected");
            }
            pair.Key.Session.Send(json);
        }

        public virtual void Broadcast(object message)
        {
            MessageWrapper wrapper = PackMessage(message);
            string json = JsonConvert.SerializeObject(wrapper);
            foreach (KeyValuePair<WebSocketUser, User> keyValuePair in onlineUsers)
            {
                if (keyValuePair.Value == null)
                {
                    continue; // do nothing with unautorized
                }
                keyValuePair.Key.Session.Send(json);
            }
        }

        protected void RegisterUser(ISession session)
        {
            WebSocketUser user = new WebSocketUser { Session = session };
            onlineUsers.TryAdd(user, null);
        }

        protected void ParseIncoming(ISession session, string jsonString)
        {
            JObject parsed; // TODO: test bullshit exception
            try
            {
                parsed = JObject.Parse(jsonString);
            }
            catch (JsonReaderException)
            {
                Console.WriteLine("Error parsing incoming json " + jsonString);
                return;
            }
            JToken typeToken = parsed.SelectToken("Type");
            // TODO: check token is null
            string typeName = typeToken.ToObject<string>();
            Console.WriteLine("Got message of type " + typeName);
            Type realType = GetTypeByName(typeName);
            object message = JsonConvert.DeserializeObject(parsed.SelectToken("Msg").ToString(), realType);
            if (message is AuthMessage)
            {
                CheckAuth(session, (AuthMessage)message);
            }
        }

        private void CheckAuth(ISession session, AuthMessage message)
        {
            Console.WriteLine("Checking session id " + message.SessionId);

            User user = userDatabase[message.SessionId];
            if (user == null)
            {
                Console.WriteLine("Unknown login " + message.SessionId);
                return;
            }

            WebSocketUser u = onlineUsers.Keys.Single(o => o.Session.ClientAddress.Equals(session.ClientAddress));
            onlineUsers[u] = user;
        }

        protected void LogoutUser(ISession session)
        {
            var user = onlineUsers.Keys.Single(o => o.Session.ClientAddress.Equals(session.ClientAddress));

            User trash; // Concurrent dictionaries make things weird

            onlineUsers.TryRemove(user, out trash);
        }
    }
}

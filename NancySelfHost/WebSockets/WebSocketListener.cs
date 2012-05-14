using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alchemy;
using Alchemy.Classes;
using NancySelfHost.Auth;
using NancySelfHost.WebSockets.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NancySelfHost.WebSockets
{
    public class WebSocketListener
    {
        private readonly WebSocketServer server;
        /// <summary>
        /// Store the list of online users. Wish I had a ConcurrentList. 
        /// </summary>
        private readonly ConcurrentDictionary<WebSocketUser, User> onlineUsers = new ConcurrentDictionary<WebSocketUser, User>();
        private readonly IDictionary<string, Type> registeredTypes = new Dictionary<string, Type>();
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly Task logPusher;
        private IUserDatabase userDatabase;

        public WebSocketListener(int port, IPAddress ipAddress, TimeSpan timeout, IUserDatabase userDatabase)
        {
            this.userDatabase = userDatabase;

            server = new WebSocketServer(port, ipAddress)
                         {
                             TimeOut = timeout,
                             FlashAccessPolicyEnabled = true,
                             OnConnected = OnConnected,
                             OnDisconnect = OnDisconnect,
                             OnReceive = OnReceive,
                             OnSend = OnSend
                         };

            CancellationToken ct = tokenSource.Token;
            logPusher = new Task(() =>
                                     {
                                         while (!ct.IsCancellationRequested)
                                         {
                                             LogMessage message = new LogMessage
                                                                      {
                                                                          MessageText = "Another message",
                                                                          When = DateTime.Now
                                                                      };
                                             Broadcast(message);
                                             Thread.Sleep(100);
                                         }
                                     }, ct);

            // TODO: move to external init
            Register<AuthMessage>();
        }

        private string GetTypeName(Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeof(WebSocketMessageAttribute), false);
            if (attrs.Length < 1)
            {
                throw new NotImplementedException("Websocket message must have WebSocketMessage attribute");
            }
            WebSocketMessageAttribute attr = (WebSocketMessageAttribute)attrs[0];
            return attr.TypeName;
        }

        public void Register<T>() where T: class
        {
            registeredTypes.Add(GetTypeName(typeof(T)), typeof(T));
        }

        private MessageWrapper PackMessage(object message)
        {
            return new MessageWrapper {Message = message, TypeName = GetTypeName(message.GetType())};
        }

        public void Send(object message, User user)
        {
            MessageWrapper wrapper = PackMessage(message);
            string json = JsonConvert.SerializeObject(wrapper);

            KeyValuePair<WebSocketUser, User> pair = onlineUsers.FirstOrDefault(p => p.Value == user);
            if (pair.Value == null)
            {
                Console.WriteLine("User " + user + " is not connected");
            }
            pair.Key.Context.Send(json);
        }

        public void Broadcast(object message)
        {
            MessageWrapper wrapper = PackMessage(message);
            string json = JsonConvert.SerializeObject(wrapper);
            foreach (KeyValuePair<WebSocketUser, User> keyValuePair in onlineUsers)
            {
                if (keyValuePair.Value == null)
                {
                    continue; // do nothing with unautorized
                }
                keyValuePair.Key.Context.Send(json);
            }
        }

        public void Start()
        {
            server.Start();
            logPusher.Start();
        }

        public void Stop()
        {
            tokenSource.Cancel();
            server.Stop();
        }

        private void OnConnected(UserContext context)
        {
            Console.WriteLine("Client Connection From : " + context.ClientAddress);

            WebSocketUser user = new WebSocketUser {Context = context};
            onlineUsers.TryAdd(user, null);
        }

        private void OnReceive(UserContext context)
        {
            Console.WriteLine("Received Data From :" + context.ClientAddress);

            string jsonString = context.DataFrame.ToString();
            JObject parsed; // TODO: test bullshit exception
            try
            {
                parsed = JObject.Parse(jsonString);
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine("Error parsing incoming json " + jsonString);
                return;
            }
            JToken typeToken = parsed.SelectToken("Type");
            // TODO: check token is null
            string typeName = typeToken.ToObject<string>();
            Console.WriteLine("Got message of type " + typeName);
            Type realType = registeredTypes[typeName];
            object message = JsonConvert.DeserializeObject(parsed.SelectToken("Msg").ToString(), realType);
            if (message is AuthMessage)
            {
                CheckAuth(context, (AuthMessage) message);
            }
        }

        private void CheckAuth(UserContext context, AuthMessage message)
        {
            Console.WriteLine("Checking session id " + message.SessionId);

            User user = userDatabase[message.SessionId];
            if (user == null)
            {
                Console.WriteLine("Unknown login " + message.SessionId);
                return;
            }

            WebSocketUser u = onlineUsers.Keys.Single(o => o.Context.ClientAddress == context.ClientAddress);
            onlineUsers[u] = user;
        }

        private void OnDisconnect(UserContext context)
        {
            Console.WriteLine("Client Disconnected : " + context.ClientAddress);
            var user = onlineUsers.Keys.Single(o => o.Context.ClientAddress == context.ClientAddress);

            User trash; // Concurrent dictionaries make things weird

            onlineUsers.TryRemove(user, out trash);
        }

        private void OnSend(UserContext context)
        {
            Console.WriteLine("Data sent to : " + context.ClientAddress);
        }

    }
}
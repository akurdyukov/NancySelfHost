using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NancySelfHost.Auth;

namespace NancySelfHost.WebSockets
{
    public interface IWebSocketServer
    {
        /// <summary>
        /// Register message type for receiving from client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Register<T>() where T: class;

        /// <summary>
        /// Send message to given user. Does nothing if user is not logged in.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user"></param>
        void Send(object message, User user);

        /// <summary>
        /// Send message to all connected users
        /// </summary>
        /// <param name="message"></param>
        void Broadcast(object message);

        /// <summary>
        /// Start websocket server
        /// </summary>
        void Start();

        /// <summary>
        /// Stop websocket server
        /// </summary>
        void Stop();
    }
}

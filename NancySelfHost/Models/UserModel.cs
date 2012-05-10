using System;

namespace NancySelfHost.Models
{
    public class UserModel
    {
        public string Username { get; private set; }
        public Guid Id { get; private set; }

        public UserModel(string username, Guid id)
        {
            Id = id;
            Username = username;
        }
    }
}

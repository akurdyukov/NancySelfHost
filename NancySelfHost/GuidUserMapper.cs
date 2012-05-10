using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.Authentication.Forms;
using Nancy.Security;
using NancySelfHost.Auth;

namespace NancySelfHost
{
    public class GuidUserMapper : IUserMapper
    {
        private readonly IUserDatabase userDatabase;
        private readonly IDictionary<Guid, User> cachedUsers = new Dictionary<Guid, User>();

        public GuidUserMapper(IUserDatabase userDatabase)
        {
            this.userDatabase = userDatabase;
        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier)
        {
            if (!cachedUsers.ContainsKey(identifier))
                return null;
            User info = cachedUsers[identifier];
            return new UserIndentity {UserName = info.Login, Claims = new List<string> {info.Role.ToString()}, AuthId = identifier};
        }

        public Guid? ValidateUser(string username, string password)
        {
            User user = userDatabase[username];

            if (user == null)
                return null;

            if (user.Password != password)
                return null;

            var oldPair = cachedUsers.FirstOrDefault(pair => pair.Value.Login == username);
            if (oldPair.Value != null)
                return oldPair.Key;

            Guid result = Guid.NewGuid();
            cachedUsers.Add(result, user); // TODO: check this user alread exist with other guid
            return result;
        }
    }
}

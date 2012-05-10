using System.Collections.Generic;

namespace NancySelfHost.Auth
{
    public class UserDatabase : IUserDatabase
    {
        private readonly IDictionary<string, User> users = new Dictionary<string, User>();

        public UserDatabase()
        {
            users.Add("test1", new User("test1", "pass1", UserRoles.User));
            users.Add("man1", new User("man1", "pass2", UserRoles.Manager));
        }

        public User this[string login]
        {
            get
            {
                if (users.ContainsKey(login))
                    return users[login];
                return null;
            }
        }
    }
}

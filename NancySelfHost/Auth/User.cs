namespace NancySelfHost.Auth
{
    public class User
    {
        private readonly string login;
        private readonly string password;
        private readonly UserRoles role;

        public User(string login, string password, UserRoles role)
        {
            this.login = login;
            this.password = password;
            this.role = role;
        }

        public string Password
        {
            get { return password; }
        }

        public string Login
        {
            get { return login; }
        }

        public UserRoles Role
        {
            get { return role; }
        }
    }
}

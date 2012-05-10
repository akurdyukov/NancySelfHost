namespace NancySelfHost.Auth
{
    public interface IUserDatabase
    {
        /// <summary>
        /// Return User by login. Returns null for unknown user.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        User this[string login] { get; }
    }
}

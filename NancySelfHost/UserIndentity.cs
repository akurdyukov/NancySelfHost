using System;
using System.Collections.Generic;
using Nancy.Security;

namespace NancySelfHost
{
    public class UserIndentity : IUserIdentity
    {
        public string UserName { get; set; }
        public IEnumerable<string> Claims { get; set; }
        public Guid AuthId { get; set; }
    }
}

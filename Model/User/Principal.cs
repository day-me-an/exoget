using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace Exo.Exoget.Model.User
{
    public class Principal : System.Security.Principal.GenericPrincipal
    {
        private uint userId;

        public Principal(System.Security.Principal.IIdentity identity, string[] roles)
            : base(identity, roles)
        {
        }

        public Principal(System.Security.Principal.IIdentity identity)
            : base(identity, null)
        {
        }

        public uint UserId
        {
            get { return userId == 0 ? (userId = UInt32.Parse(Identity.Name)) : userId; }
        }
    }
}

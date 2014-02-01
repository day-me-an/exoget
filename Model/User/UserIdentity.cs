using System;
using System.Collections.Generic;
using System.Text;

namespace Exo.Exoget.Model.User
{
    public class UserIdentity : System.Security.Principal.IIdentity
    {
        private readonly uint id;

        public uint Id
        {
            get { return id; }
        } 

        private readonly string name;

        public UserIdentity(uint id, string name)
        {
            this.id = id;
            this.name = name;
        }

        #region IIdentity Members

        public string AuthenticationType
        {
            get { return "Exo Custom"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public string Name
        {
            get { return name; }
        }

        #endregion


    }
}

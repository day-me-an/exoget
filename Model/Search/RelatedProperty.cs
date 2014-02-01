using System;
using System.Collections.Generic;
using System.Text;

namespace Exo.Exoget.Model.Search
{
    public class RelatedProperty
    {
        private uint id;
        private string value;
        private uint occurences;

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public uint Occurences
        {
            get { return occurences; }
            set { occurences = value; }
        }
    }
}

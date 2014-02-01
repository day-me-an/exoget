using System;
using System.Collections.Generic;
using System.Text;
using Exo.Extensions;

namespace Exo.Exoget.Model.Search
{
    [Serializable]
    public class IndexPropertyInfo
    {
        private readonly uint id;
        private readonly IndexPropertyType type;
        private readonly string value;
        private string valueUri;

        public IndexPropertyInfo(uint id, IndexPropertyType type, string value)
        {
            this.id = id;
            this.type = type;
            this.value = value;
        }

        public IndexPropertyInfo(IndexPropertyType type, string value)
            : this(0, type, value)
        {
        }

        public uint Id
        {
            get { return id; }
        }

        public IndexPropertyType Type
        {
            get { return type; }
        }

        public string Value
        {
            get { return value; }
        }

        public string ValueUri
        {
            get { return valueUri ?? (valueUri = String.Join("-", value.Tokenize())); }
        }

        public override string ToString()
        {
            return String.Format("ID={0}, Type={1}, Value={2}", id, type, value);
        }
    }
}

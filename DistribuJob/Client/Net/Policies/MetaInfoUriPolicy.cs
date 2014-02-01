using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DistribuJob.Client.Net.Policies
{
    [Serializable]
    public class MetaInfoUriPolicy : UriPolicy
    {
        private int startPos, upwards, downwards;

        public MetaInfoUriPolicy(uint id, UriPolicyType type, string regex)
            : base(id, type, regex)
        {
        }

        public MetaInfoUriPolicy(uint id, string regex)
            : base(id, UriPolicyType.LINE_META_INFO, regex)
        {
        }

        public int StartPos
        {
            get { return startPos; }
            set { startPos = value; }
        }

        public int Upwards
        {
            get { return upwards; }
            set { upwards = value; }
        }

        public int Downwards
        {
            get { return downwards; }
            set { downwards = value; }
        }
    }
}

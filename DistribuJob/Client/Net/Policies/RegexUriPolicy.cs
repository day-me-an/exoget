using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DistribuJob.Client.Net.Policies
{
    [Serializable]
    public class RegexUriPolicy : UriPolicy
    {
        [NonSerialized]
        private Regex regex;

        public RegexUriPolicy(uint id, UriPolicyType type, string uriRegex)
            : base(id, type, uriRegex)
        {
        }

        public bool Matches(string input)
        {
            return Regex.IsMatch(input);
        }

        public bool Matches(Uri input)
        {
            return Matches(input.ToString());
        }

        public Regex Regex
        {
            get { return regex ?? (regex = new Regex(StringValue)); }
            set { regex = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DistribuJob.Client.Net.Policies
{
    [Serializable]
    public class ValueFromUriRegexUriPolicy : RegexUriPolicy
    {
        private readonly string value;

        public ValueFromUriRegexUriPolicy(uint id, UriPolicyType type, string uriRegex, string value)
            : base(id, type, uriRegex)
        {
            this.value = value;
        }

        public string GetValueFromUri(string uri)
        {
            Match match = Regex.Match(uri);

            if (match.Success)
                return String.Format(value, match.Groups["value"].Value);

            else
                return null;
        }

    }
}

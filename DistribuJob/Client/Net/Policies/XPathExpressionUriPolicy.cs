using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace DistribuJob.Client.Net.Policies
{
    [Serializable]
    public class XPathExpressionUriPolicy : UriPolicy
    {
        [NonSerialized]
        private XPathExpression expression;

        public XPathExpressionUriPolicy(uint id, UriPolicyType type, string uriRegex)
            : base(id, type, uriRegex)
        {
        }

        public XPathExpression XPathExpression
        {
            get { return expression ?? (expression = XPathExpression.Compile(StringValue)); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using DistribuJob.Client.Net.Policies;
using System.Xml.Serialization;
using System.Collections;
using C5;
using System.Net;
using System.IO;
using DistribuJob.Client.Processors;
using DistribuJob.Client.Properties;

namespace DistribuJob.Client.Net
{
    [Serializable]
    public class Server
    {
        public readonly uint id;
        private Uri uri;
        public Exo.Net.NetworkProtocol protocol;
        private int timeout;
        public List<UriPolicy> uriPolicies;
        public byte delay;

        [NonSerialized]
        public DateTime robotsTxtLastFetched, lastRequest;
        [NonSerialized]
        public volatile ushort
            successfulRequests,
            consecutiveUnsuccessfulRequests,
            unsuccessfulRequests,
            consecutiveTimeouts,
            consecutiveLookupFailures,
            consecutiveSlowFetches;

        public Server()
        {
        }

        public Server(uint id, List<UriPolicy> uriPolicies)
        {
            this.id = id;
            this.uriPolicies = uriPolicies;
        }

        public override int GetHashCode()
        {
            return (int)id;
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }

        public override string ToString()
        {
            return Uri.ToString();
        }

        public void IncrementSuccessfullRequests()
        {
            lastRequest = DateTime.Now;
            successfulRequests++;
            consecutiveUnsuccessfulRequests = 0;
        }

        public void IncrementUnsuccessfulRequests()
        {
            lastRequest = DateTime.Now;
            unsuccessfulRequests++;
            consecutiveUnsuccessfulRequests++;
        }

        public void LoadRobotsTxt(Job job)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Uri + "robots.txt");
            req.KeepAlive = false;
            req.Timeout = Timeout;
            req.ReadWriteTimeout = Timeout;
            req.UserAgent = String.Format(Settings.Default.WebUseragent, System.Windows.Forms.Application.ProductVersion);
            req.Referer = Uri.ToString();

            HttpWebResponse res = null;

            try
            {
                res = (HttpWebResponse)req.GetResponse();

                if (res.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader sr = new StreamReader(res.GetResponseStream());
                    string robotsTxt = sr.ReadToEnd();
                    RobotsExclusionParser.Parse(robotsTxt, this);
                }
            }
            finally
            {
                if (res != null)
                    res.Close();
            }

            robotsTxtLastFetched = DateTime.Now;
        }

        public UriPolicy[] FilterUriPolicies(UriPolicy.UriPolicyType type, Uri uri)
        {
            List<UriPolicy> filteredUriPolicies = new List<UriPolicy>();

            foreach (UriPolicy uriPolicy in uriPolicies)
                if (uriPolicy.type == type && uriPolicy.MatchesUri(uri))
                    filteredUriPolicies.Add(uriPolicy);

            if (filteredUriPolicies.Count > 0)
                return filteredUriPolicies.ToArray();

            else
                return null;
        }

        public UriPolicy FilterUriPolicy(UriPolicy.UriPolicyType type, Uri uri)
        {
            UriPolicy[] filteredUriPolicies = FilterUriPolicies(type, uri);

            if (filteredUriPolicies != null)
                return filteredUriPolicies[0];

            else
                return null;
        }

        public static bool ValidateServer(Server server)
        {
            foreach (UriPolicy uriPolicy in server.uriPolicies)
            {
                if (uriPolicy.IsExpired)
                {
                    Console.WriteLine(uriPolicy + " is expired");
                    return false;
                }
            }

            return true;
        }

        public bool AcceptUri(Uri uri)
        {
            return (FilterUriPolicy(UriPolicy.UriPolicyType.DISALLOW, uri) == null && (FilterUriPolicy(UriPolicy.UriPolicyType.ALLOW, uri) == null || FilterUriPolicy(UriPolicy.UriPolicyType.ALLOW, uri) != null))
                || (FilterUriPolicy(UriPolicy.UriPolicyType.DISALLOW, uri) != null && (FilterUriPolicy(UriPolicy.UriPolicyType.ALLOW, uri) != null));
        }

        public bool IsNextRequestLastChance
        {
            get { return consecutiveUnsuccessfulRequests == (Settings.Default.Server_MaxConsecutiveUnsuccessfulRequests - 1); }
        }

        public bool IsDelayComplete
        {
            get { return ((TimeSpan)DateTime.Now.Subtract(lastRequest)).TotalSeconds >= delay; }
        }

        public bool IsRobotsTxtExpired
        {
            get { return (DateTime.Now - robotsTxtLastFetched) >= Settings.Default.Server_RobotsTxtExpire; }
        }

        public int Timeout
        {
            get { return timeout == 0 ? Settings.Default.Server_DefaultTimeout : timeout; }
            set { timeout = value; }
        }

        public Uri Uri
        {
            get { return uri; }
            set { uri = value; }
        }
    }
}

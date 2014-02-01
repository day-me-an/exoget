using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DistribuJob.Client.Properties;

namespace DistribuJob.Client.Net.Policies
{
    static class RobotsExclusionParser
    {
        private static readonly Regex junk = new Regex("#|\\s");
        private static readonly Regex crawlDelayRegex = new Regex("([0-9]^3)");

        public static void Parse(string robotsTxtStr, Server server)
        {
            string[] lines = robotsTxtStr.Split('\n');

            if (lines.Length != 0)
            {
                int userAgentStart = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = junk.Replace(lines[i], "").ToLower().Trim();

                    if (lines[i].StartsWith("user-agent:") &&
                            (lines[i].IndexOf("*", 11) == 11 || lines[i].IndexOf("distribujob", 11) != -1))
                    {
                        userAgentStart = i;

                        bool rule = true;
                        int j = userAgentStart + 1;
                        while (rule && j < lines.Length)
                        {
                            lines[j] = junk.Replace(lines[j], "").ToLower().Trim();

                            if (lines[j].StartsWith("disallow:") && lines[j].Length > 9)
                            {
                                try
                                {
                                    server.uriPolicies.Add(
                                        new UriPolicy(
                                        UriPolicy.DynamicPolicyId,
                                        UriPolicy.UriPolicyType.DISALLOW,
                                        lines[j].Substring(9)
                                        ));
                                }
                                catch { }
                            }
                            else if (lines[j].StartsWith("allow:") && lines[j].Length > 6)
                            {
                                try
                                {
                                    server.uriPolicies.Add(
                                        new UriPolicy(
                                        UriPolicy.DynamicPolicyId,
                                        UriPolicy.UriPolicyType.ALLOW,
                                        lines[j].Substring(6)
                                        ));
                                }
                                catch { }
                            }
                            else if (lines[j].StartsWith("crawl-delay:") && lines[j].Length > 12)
                            {
                                try
                                {
                                    byte crawlDelay = Convert.ToByte(lines[j].Substring(12, 3));

                                    if (crawlDelay > Settings.Default.Server_MaxDelay)
                                        server.delay = Settings.Default.Server_MaxDelay;

                                    else if (crawlDelay >= 0)
                                        server.delay = (byte)crawlDelay;
                                }
                                catch
                                {
                                    server.delay = 0;
                                }
                            }
                            else if (lines[j].StartsWith("user-agent:"))
                            {
                                i = j - 1;
                                rule = false;
                            }

                            j++;
                        }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Exo.Collections;
using Exo.Misc;

namespace DistribuJob.Client.Net
{
    public class Servers
    {
        private static object getServerLock = new object(), setServerStatusLock = new object();

        private static readonly Cache<uint, Server> serverCache = new Cache<uint, Server>(TimeSpan.FromHours(4), 10000, TimeSpan.FromMinutes(1), new Cache<uint, Server>.CacheCustomValidator(Server.ValidateServer));
        private static readonly HashSet<uint> setStatusServerIds = new HashSet<uint>();

        public Server this[Job job]
        {
            get
            {
                Server server;

                lock (getServerLock)
                {
                    if (!serverCache.TryGetValue(job.ServerId, out server))
                    {
                        server = Dj.Djs.GetServer(job.ServerId);
                        server.uriPolicies.Add(new DistribuJob.Client.Net.Policies.UriPolicy(0, DistribuJob.Client.Net.Policies.UriPolicy.UriPolicyType.LINKS_MEDIA_ONLY, "(.*)"));

                        serverCache[job.ServerId] = server;
                    }
                }

                return server;
            }
        }

        public void SetServerStatus(uint serverId, ServerStatus status)
        {
            lock (setServerStatusLock)
            {
                if (!setStatusServerIds.Contains(serverId))
                {
                    Dj.Djs.SetServerStatus(serverId, status);
                    setStatusServerIds.Add(serverId);
                }
            }
        }
    }
}

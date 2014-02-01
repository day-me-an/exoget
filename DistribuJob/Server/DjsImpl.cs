using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistribuJob.Service
{
    public class DjsImpl : IDjs
    {
        #region IDjs Members

        public void CreateDocument(IEnumerable<Uri> uris)
        {
            throw new NotImplementedException();
        }

        public StringBuilder CreateDocumentSql(IEnumerable<Uri> uris, Exo.Web.DocumentType forceType)
        {
            throw new NotImplementedException();
        }

        public StringBuilder CreateDocumentSql(IEnumerable<Uri> uris)
        {
            throw new NotImplementedException();
        }

        public void Export(DistribuJob.Client.Job[] jobs)
        {
            throw new NotImplementedException();
        }

        public uint GetDomainId(string domain)
        {
            throw new NotImplementedException();
        }

        public DistribuJob.Client.Net.Server GetServer(uint serverId)
        {
            throw new NotImplementedException();
        }

        public uint GetServerId(Uri uri)
        {
            throw new NotImplementedException();
        }

        public uint GetServerId(Exo.Net.NetworkProtocol protocol, int port, uint domainId)
        {
            throw new NotImplementedException();
        }

        public List<DistribuJob.Client.Net.Policies.UriPolicy> GetUriPolicies(uint serverId)
        {
            throw new NotImplementedException();
        }

        public DistribuJob.Client.Job[] Import(int count)
        {
            throw new NotImplementedException();
        }

        public DistribuJob.Client.Job[] Import(int count, bool lockResults)
        {
            throw new NotImplementedException();
        }

        public void SetServerStatus(uint serverId, DistribuJob.Client.Net.ServerStatus status)
        {
            throw new NotImplementedException();
        }

        public void SetUriPolicyValue(uint uriPolicyId, object value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetDomainId(string domain, out uint domainId)
        {
            throw new NotImplementedException();
        }

        public bool TryGetServerId(Uri uri, out uint serverId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace DistribuJob.Service
{
    [ServiceContract] 
    public interface IDjs
    {
        [OperationContract]
        void CreateDocument(System.Collections.Generic.IEnumerable<Uri> uris);

        [OperationContract]
        System.Text.StringBuilder CreateDocumentSql(System.Collections.Generic.IEnumerable<Uri> uris, Exo.Web.DocumentType forceType);

        [OperationContract]
        System.Text.StringBuilder CreateDocumentSql(System.Collections.Generic.IEnumerable<Uri> uris);

        [OperationContract]
        void Export(DistribuJob.Client.Job[] jobs);

        [OperationContract]
        uint GetDomainId(string domain);

        [OperationContract]
        DistribuJob.Client.Net.Server GetServer(uint serverId);

        [OperationContract]
        uint GetServerId(Uri uri);

        [OperationContract]
        uint GetServerId(Exo.Net.NetworkProtocol protocol, int port, uint domainId);

        [OperationContract]
        System.Collections.Generic.List<DistribuJob.Client.Net.Policies.UriPolicy> GetUriPolicies(uint serverId);

        [OperationContract]
        DistribuJob.Client.Job[] Import(int count);

        [OperationContract]
        DistribuJob.Client.Job[] Import(int count, bool lockResults);

        [OperationContract]
        void SetServerStatus(uint serverId, DistribuJob.Client.Net.ServerStatus status);

        [OperationContract]
        void SetUriPolicyValue(uint uriPolicyId, object value);

        [OperationContract]
        bool TryGetDomainId(string domain, out uint domainId);

        [OperationContract]
        bool TryGetServerId(Uri uri, out uint serverId);
    }
}

using System;
using System.Runtime.Serialization;
using DistribuJob.Client.Extracts;
using Exo.Misc;
using Exo.Web;

namespace DistribuJob.Client
{
    [Serializable]
    //[DataContract]
    public partial class Job
    {
        private uint id;
        private uint serverId;
        private bool changedServerId;
        private string path;
        private bool changedPath;
        private FetchStatus fetchStatus;
        private uint lastModified;
        private long contentLength;
        private DocumentType type;
        private DocumentFormat format;
        private Extract extract;

        public Job()
        {
        }

        public Job(uint id)
        {
            this.id = id;
        }

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }

        public uint ServerId
        {
            get { return serverId; }

            set
            {
                changedServerId = serverId != 0 && value != serverId;
                serverId = value;
            }
        }

        public string Path
        {
            get { return path; }

            set
            {
                changedPath = path != null && value != path;
                path = value;
            }
        }

        public uint LastModified
        {
            get { return lastModified; }
            set { lastModified = value; }
        }

        public DocumentFormat Format
        {
            get { return format; }
            set { format = value; }
        }

        public DocumentType Type
        {
            get
            {
                if (type == DocumentType.None)
                {
                    DocumentFormatInfo formatInfo;

                    type = UriUtil.TryGetFormatInfoFromFormat(format, out formatInfo) ? formatInfo.Type : DocumentType.Unknown;
                }

                return type;
            }

            set { type = value; }
        }

        public long ContentLength
        {
            get { return contentLength; }
            set { contentLength = value; }
        }

        public FetchStatus FetchStatus
        {
            get { return fetchStatus; }
            set { fetchStatus = value; }
        }

        public bool HasChangedServerId
        {
            get { return changedServerId; }
        }

        public bool HasChangedPath
        {
            get { return changedPath; }
        }

        public Extract Extract
        {
            get { return extract; }
            set { extract = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using DistribuJob.Client.Extracts.Links;
using System.Runtime.Serialization;

namespace DistribuJob.Client.Extracts
{
    [Serializable]
    public abstract class Extract : IndexPropertiesBase
    {
        private Link[] links;

        [NonSerialized]
        private List<Link> linkList;
        [NonSerialized]
        private Dictionary<LinkType, Link[]> linksByTypeCache;

        [OnSerializing]
        private void MoveLinksFromList(StreamingContext context)
        {
            if (linkList != null)
            {
                if (links != null && links.Length > 0)
                    links = Exo.Array.Join<Link>(links, linkList.ToArray());

                else
                    links = linkList.ToArray();
            }
        }

        public T[] FilterLinksByType<T>(LinkType type) where T : Link
        {
            Link[] linksByType;

            if (linksByTypeCache == null || !linksByTypeCache.TryGetValue(type, out linksByType))
            {
                List<T> linksByTypeList = new List<T>();

                foreach (Link link in Links)
                    if (link.Type == type)
                        linksByTypeList.Add((T)link);

                linksByType = linksByTypeList.ToArray();

                if (linksByType.Length > 0)
                {
                    if (linksByTypeCache == null)
                        linksByTypeCache = new Dictionary<LinkType, Link[]>();

                    linksByTypeCache[type] = linksByType;
                }
            }

            return (T[])linksByType;
        }

        public Link[] Links
        {
            get { return links; }
            set { links = value; }
        }

        internal List<Link> LinkList
        {
            get
            {
                if (linkList == null)
                {
                    if (links != null)
                        throw new InvalidOperationException("Cannot create a Link when links is not null.");

                    linkList = new List<Link>();
                }

                return linkList;
            }
        }
    }
}
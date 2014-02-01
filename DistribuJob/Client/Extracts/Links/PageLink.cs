using System;
using System.Collections.Generic;
using System.Text;
using DistribuJob.Client.Processors.Html.Lines;

namespace DistribuJob.Client.Extracts.Links
{
    [Serializable]
    public class PageLink : Link
    {
        public const int MaxRepeatingLinkText = 3;

        public PageLink(Uri targetUri)
            : base(LinkType.Page, targetUri)
        {
        }

        public override void AddIndexProperties()
        {
            return;
        }
    }
}

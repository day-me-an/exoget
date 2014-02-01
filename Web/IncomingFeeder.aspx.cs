using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using Exo.Exoget.Model.Media;
using System.Collections.Generic;
using System.IO;
using Exo.Exoget.Web.Controls;
using Exo.Exoget.Model.Search;

public partial class IncomingFeeder : CommonPage
{
    public IncomingFeeder()
    {
        Load += Page_Load;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        uint start;
        byte count;

        if (!UInt32.TryParse(Request.QueryString["start"], out start))
            return;

        if (!Byte.TryParse(Request.QueryString["count"], out count) || count > 10)
            return;

        IList<MediaInfo> medias;

        MediaManager mediaManger = new MediaManager(DatabaseConnection);
        medias = mediaManger.GetMedia(String.Format("SELECT mediaId FROM incomingepisodes ORDER BY pubdate DESC LIMIT {0},{1}", start, count), MediaInfoTypes.Properties);

        if (medias.Count == 0)
            return;

        foreach (MediaInfo media in medias)
        {
            if (media.Properties == null)
                return;

            media.Properties =
                (from prop in media.Properties
                 where prop.Type == IndexPropertyType.Pubdate
                 select prop).ToArray();
        }

        using (TextWriter sw = new StringWriter())
        using (HtmlTextWriter writer = new HtmlTextWriter(sw))
        {
            writer.Write("<div>");

            foreach (MediaInfo media in medias.Reverse())
            {
                MediaRow mediaRow = (MediaRow)LoadControl("~/UserControls/MediaRow.ascx");
                mediaRow.Media = media;

                mediaRow.RenderControl(writer);
            }

            writer.Write("</div>");

            Controls.Add(new LiteralControl(sw.ToString()));
        }
    }
}

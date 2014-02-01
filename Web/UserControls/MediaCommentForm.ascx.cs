using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class MediaCommentForm : System.Web.UI.UserControl
{
    private uint mediaId;

    protected void Page_Load(object sender, EventArgs e)
    {
    }

    public uint MediaId
    {
        get { return mediaId; }
        set { mediaId = value; }
    }
}


using System;
using System.Web.UI.HtmlControls;

public partial class _Default : CommonPage
{
    public _Default()
    {
        Load += SetTitle;
    }

    private void SetTitle(object sender, EventArgs e)
    {
        Title = "exoGet - MP3 Search Engine";

        Header.Controls.Add(
            new HtmlMeta()
            {
                Name = "description",
                Content = "exoGet is a multimedia & MP3 search engine that allows you to cut through the hard-to-navigate, slow-loading and unfamiliar places on the web that contain the media you want by making everything searchable in a smart, fast and clean interface."
            });
    }
}

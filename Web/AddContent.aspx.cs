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
using Exo.Misc;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MySql.Data.MySqlClient;
using Exo.Extensions;

public partial class AddContent : CommonPage
{
    private HashSet<string> validUrls = new HashSet<string>();

    public AddContent()
    {
        this.PreRender += new EventHandler(SaveUrls);
    }

    private void SaveUrls(object sender, EventArgs e)
    {
        if (IsPostBack && validUrls.Count > 0)
        {
            StringBuilder sb = new StringBuilder("INSERT IGNORE INTO submittedurls (url) VALUES ");

            foreach (string url in validUrls)
                sb.Append("(" + url.SqlEscape() + "),");

            sb = sb.Remove(sb.Length - 1, 1);

            using (MySqlCommand command = new MySqlCommand(sb.ToString(), DatabaseConnection))
            {
                command.BeginExecuteNonQuery();
            }

            instructions.Visible = false;
            urlForm.Visible = false;

            success.Visible = true;
        }
    }

    protected void OnUrlValidator(object sender, ServerValidateEventArgs args)
    {
        if (String.IsNullOrEmpty(args.Value))
        {
            args.IsValid = false;
            return;
        }

        string[] lines = args.Value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 1)
            args.IsValid = false;

        else
        {
            HashSet<string> urlSet = new HashSet<string>(lines);
            uint i = 1;

            foreach (string url in lines)
            {
                Uri uri;

                if (!Uri.TryCreate(url, UriKind.Absolute, out uri) || !UriUtil.TryValidateUri(ref uri))
                {
                    ((CustomValidator)sender).Text = String.Format("The URL on line {0} was invalid", i);
                    args.IsValid = false;

                    return;
                }
                else
                    validUrls.Add(url);

                i++;
            }

            args.IsValid = true;
        }
    }
}

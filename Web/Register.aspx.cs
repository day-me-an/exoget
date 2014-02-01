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
using System.Collections.Generic;
using Exo.Collections;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Net;
using Exo.Exoget.Model.User;

public partial class Register : CommonPage
{
    public Register()
    {
        PreRender += new EventHandler(Register_PreRender);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "exoGet - " + Resources.Resource.Register;

        userForm.User = new UserInfo();
    }

    void Register_PreRender(object sender, EventArgs e)
    {
        if (userForm.Success)
            accountCreated.Visible = true;
    }
}
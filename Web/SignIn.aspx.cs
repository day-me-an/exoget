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
using Exo.Exoget.Model.User;
using System.Security.Principal;
using Exo.Exoget.Web;

public partial class SignIn : CommonPage
{
    public SignIn()
    {
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (User.Identity.IsAuthenticated && Request.UrlReferrer != null && Request.UrlReferrer.Host == Request.ServerVariables["HTTP_HOST"])
            Response.Redirect(Request.Headers["Referer"], true);

        if (IsPostBack)
            Validate();

        else if (Request.UrlReferrer != null && Request.UrlReferrer.Host == Request.ServerVariables["HTTP_HOST"])
            returnUrl.Value = Request.Headers["Referer"];

        Page.Title = "exoGet - " + Resources.Resource.SignIn;
    }

    protected void ValidateLogin(object sender, ServerValidateEventArgs args)
    {
        IPrincipal newUser = new UserManager(DatabaseConnection).ValidateLogin(this.userName.Value, this.password.Value);

        args.IsValid = newUser != null;

        if (newUser != null)
        {
            Helper.SignIn(newUser);

            if (!String.IsNullOrEmpty(returnUrl.Value))
                Response.Redirect(returnUrl.Value);

            else
                Response.Redirect("/");
        }
    }
}

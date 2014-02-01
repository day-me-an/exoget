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

public partial class UserSettings : CommonPage
{
    private UserManager userManager;

    public UserSettings()
    {
        PreLoad += UserSettings_PreLoad;
        PreRender += UserSettings_PreRender;
    }

    private void UserSettings_PreLoad(object sender, EventArgs e)
    {
        if (User.Identity.IsAuthenticated)
            userForm.User = UserManager.GetUser(ExoUser.Id);

        else
            Response.Redirect("/signin");

        Page.Title = String.Format("Settings - {0} - exoGet", userForm.User.Username);
    }

    private void UserSettings_PreRender(object sender, EventArgs e)
    {
        if (userForm.Success)
            Response.Redirect(userForm.ReturnUrl);
    }

    private UserManager UserManager
    {
        get { return userManager ?? (userManager = new UserManager(DatabaseConnection)); }
    }
}

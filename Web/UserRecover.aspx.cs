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
using System.Net.Mail;
using Exo.Exoget.Model.User;
using System.Diagnostics;

public partial class UserRecover : CommonPage
{
    protected UserInfo user;

    public UserRecover()
    {
        PreRender += new EventHandler(SendEmail);
    }

    private void SendEmail(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            Page.Validate();

            if (IsValid)
            {
                Debug.Assert(user != null);

                MailMessage mailMessage = new MailMessage();

                mailMessage.To.Add(new MailAddress(user.Email));
                mailMessage.From = new MailAddress("service@exoget.com", "exoGet Service");
                mailMessage.Subject = "User ID Recovery Details";
                mailMessage.IsBodyHtml = true;

                mailMessage.Body = String.Format(
                    @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""en"">
<html>

<head>
<title>{0}</title>
</head>

<body>
<p>Hello,</p>
<p>Your account sign in details for <strong>{1}</strong> are as follows:</p>

<p>User Name:{2}</p>
<p>New Password:{3}</p>

<p>Welcome Back to exoGet!</p>
<p>You can change your password in the ""Settings"" section after you sign in.</p>
</body>

</html>",
            mailMessage.Subject,
            email.Text,
            user.Username,
            user.Password);

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Send(mailMessage);

                beforeSend.Visible = false;
                afterSend.Visible = true;
            }
        }
    }

    protected void ValidateEmail(object sender, ServerValidateEventArgs args)
    {
        UserManager userManager = new UserManager(DatabaseConnection);
        user = userManager.GetUserByEmail(email.Text);

        if (user != null)
        {
            user.Password = new Random().Next().ToString();
            userManager.Save(user);

            args.IsValid = true;
        }
    }
}

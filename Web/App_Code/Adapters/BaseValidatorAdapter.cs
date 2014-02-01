using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.Practices.EnterpriseLibrary.Validation.Integration.AspNet;

namespace CSSFriendly
{
    public class BaseValidatorAdapter : System.Web.UI.WebControls.Adapters.WebControlAdapter
    {
        public BaseValidatorAdapter()
        {
        }

        protected override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (!((BaseValidator)Control).IsValid)
            {
                writer.WriteBeginTag("span");
                writer.WriteAttribute("class", Control.CssClass);
                writer.Write(HtmlTextWriter.TagRightChar);
            }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (!((BaseValidator)Control).IsValid)
                writer.Write(((BaseValidator)Control).Text);
        }

        protected override void RenderEndTag(HtmlTextWriter writer)
        {
            if (!((BaseValidator)Control).IsValid)
                writer.WriteEndTag("span");
        }
    }
}
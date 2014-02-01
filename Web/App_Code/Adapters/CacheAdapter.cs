using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Text;
using System.IO;

namespace CSSFriendly
{
    public class CacheAdapter : System.Web.UI.WebControls.Adapters.WebControlAdapter
    {
        public CacheAdapter()
        {
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("hello");
            Control.RenderControl(writer);

            /*if (Control.ID != "content")
                base.Render(writer);

            else
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                HtmlTextWriter tw = new HtmlTextWriter(sw);

                Control.RenderControl(tw);
                //base.Render(tw);

                writer.Write(sb.ToString());
                writer.Write("<!--" + sb.ToString() + "-->");
            }*/
        }
    }
}
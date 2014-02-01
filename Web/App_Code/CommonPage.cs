using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using MySql.Data.MySqlClient;
using Exo.Exoget.Model.User;
using Exo.Web;
using Exo.Exoget.Model.Search;
using System.Text;
using Exo.Exoget.Model.Media;
using Exo.Exoget.Web;
using Exo.Extensions;

public class CommonPage : System.Web.UI.Page
{
    private MySqlConnection dbConn;
    private uint id;
    private ushort skey;
    private SearchOptions currentSearchOptions;

    public CommonPage()
    {
        Unload += CloseDatabaseConnection;
    }

    protected void CloseDatabaseConnection()
    {
        if (dbConn != null)
            dbConn.Dispose();
    }

    private void CloseDatabaseConnection(object sender, EventArgs e)
    {
        CloseDatabaseConnection();
    }

    public MySqlConnection DatabaseConnection
    {
        get
        {
            if (dbConn == null || dbConn.State != ConnectionState.Open)
            {
                if (dbConn != null && dbConn.State != ConnectionState.Closed)
                    dbConn.Dispose();

                dbConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["exoget"].ConnectionString);

                dbConn.Open();
            }

            return dbConn;
        }
    }

    protected uint Id
    {
        get
        {
            if (id == 0)
                UInt32.TryParse(HttpContext.Current.Request.QueryString["id"], out id);

            return id;
        }
    }

    protected ushort SKey
    {
        get
        {
            if (skey == 0)
                UInt16.TryParse(HttpContext.Current.Request.QueryString["skey"], out skey);

            return skey;
        }
    }

    protected UserIdentity ExoUser
    {
        get { return System.Threading.Thread.CurrentPrincipal.Identity as UserIdentity; }
    }

    /*
    public static string GetSearchOptionsQuery(SearchOptions options)
    {
        uint currentSearchOptions = (uint)options;

        StringBuilder sb = new StringBuilder();

        foreach (uint value in searchOptionsEnumValues)
        {
            if (value > 0
                && (value == 1 || value % 2 == 0)
                && (currentSearchOptions & value) == value
                && ((uint)SearchOptions.DurationAll & value) == 0)
            {
                if (sb.Length > 0)
                    sb.Append("&amp;");

                sb.Append("so_");
                sb.Append(value + "=1");
            }
        }

        if (sb.Length > 0)
            sb.Append("&amp;");

        sb.Append("so_duration=");

        if ((options & SearchOptions.DurationTwo) == SearchOptions.DurationTwo)
            sb.Append((uint)SearchOptions.DurationTwo);

        else if ((options & SearchOptions.DurationThree) == SearchOptions.DurationThree)
            sb.Append((uint)SearchOptions.DurationThree);

        else if ((options & SearchOptions.DurationFour) == SearchOptions.DurationFour)
            sb.Append((uint)SearchOptions.DurationFour);

        else
            sb.Append((uint)SearchOptions.DurationOne);

        return sb.ToString();
    }

    public string CurrentSearchOptionsQuery
    {
        get
        {
            if (currentSearchOptionsQuery == null && CurrentSearchOptions != SearchOptions.None)
            {
                currentSearchOptionsQuery = GetSearchOptionsQuery(Request.Path.EndsWith("MediaDetails.aspx") ? SearchOptions.All : currentSearchOptions);
            }

            return currentSearchOptionsQuery;
        }
    }
    */

    /// <summary>
    /// Returns the current search options
    /// </summary>
    public SearchOptions CurrentSearchOptions
    {
        get
        {
            if (currentSearchOptions == 0)
            {
                currentSearchOptions = Helper.CurrentSearchOptions;
            }

            return currentSearchOptions;
        }
    }

    /*
    private SearchOptions CurrentUrlSearchOptions
    {
        get
        {
            SearchOptions options = SearchOptions.None;

            // Duration
            string duration = Request.QueryString["so_duration"];
            uint durationValue;

            if (duration != null && UInt32.TryParse(duration, out durationValue))
            {
                options |= (SearchOptions)durationValue;
            }

            // Quality
            if (Request.QueryString["so_"+((uint)SearchOptions.QualityPoor).ToString()] != null)
                options |= SearchOptions.QualityPoor;

            if (Request.QueryString["so_" + ((uint)SearchOptions.QualityOk).ToString()] != null)
                options |= SearchOptions.QualityOk;

            if (Request.QueryString["so_" + ((uint)SearchOptions.QualityGood).ToString()] != null)
                options |= SearchOptions.QualityGood;

            if (Request.QueryString["so_" + ((uint)SearchOptions.QualityExcellent).ToString()] != null)
                options |= SearchOptions.QualityExcellent;

            // Format
            if (Request.QueryString["so_" + ((uint)SearchOptions.FormatMp3).ToString()] != null)
                options |= SearchOptions.FormatMp3;

            if (Request.QueryString["so_" + ((uint)SearchOptions.FormatMsMedia).ToString()] != null)
                options |= SearchOptions.FormatMsMedia;

            if (Request.QueryString["so_" + ((uint)SearchOptions.FormatRealmedia).ToString()] != null)
                options |= SearchOptions.FormatRealmedia;

            if (Request.QueryString["so_" + ((uint)SearchOptions.FormatQuicktime).ToString()] != null)
                options |= SearchOptions.FormatQuicktime;

            if (Request.QueryString["so_" + ((uint)SearchOptions.FormatMp4).ToString()] != null)
                options |= SearchOptions.FormatMp4;

            return options;
        }
    }*/
}
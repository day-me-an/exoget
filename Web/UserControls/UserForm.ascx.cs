using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Exo.Exoget.Model.User;
using Exo.Extensions;
using Exo.Exoget.Model.Search;
using Exo.Exoget.Web;

public partial class UserForm : System.Web.UI.UserControl
{
    public enum UserFormMode : byte
    {
        Create,
        Edit
    }

    protected static Dictionary<string, string> locations = Exo.Collections.Util.CreateDictionary(
    new string[] { "AF", "AL", "DZ", "AS", "AD", "AO", "AI", "AQ", "AG", "AR", "AM", "AW", "AU", "AT", "AZ", "BS", "BH", "BD", "BB", "BY", "BE", "BZ", "BJ", "BM", "BT", "BO", "BA", "BW", "BV", "BR", "IO", "BN", "BG", "BF", "BI", "KH", "CM", "CA", "CV", "KY", "CF", "TD", "CL", "CN", "CX", "CC", "CO", "KM", "CG", "CD", "CK", "CR", "CI", "HR", "CU", "CY", "CZ", "DK", "DJ", "DM", "DO", "TP", "EC", "EG", "SV", "GQ", "ER", "EE", "ET", "FK", "FO", "FJ", "FI", "FR", "GF", "PF", "TF", "GA", "GM", "GE", "DE", "GH", "GI", "GR", "GL", "GD", "GP", "GU", "GT", "GN", "GW", "GY", "HT", "HM", "VA", "HN", "HK", "HU", "IS", "IN", "ID", "IR", "IQ", "IE", "IL", "IT", "JM", "JP", "JO", "KZ", "KE", "KI", "KP", "KR", "KW", "KG", "LA", "LV", "LB", "LS", "LR", "LY", "LI", "LT", "LU", "MO", "MK", "MG", "MW", "MY", "MV", "ML", "MT", "MH", "MQ", "MR", "MU", "YT", "MX", "FM", "MD", "MC", "MN", "MS", "MA", "MZ", "MM", "NA", "NR", "NP", "NL", "AN", "NC", "NZ", "NI", "NE", "NG", "NU", "NF", "MP", "NO", "OM", "PK", "PW", "PS", "PA", "PG", "PY", "PE", "PH", "PN", "PL", "PT", "PR", "QA", "RE", "RO", "RU", "RW", "SH", "KN", "LC", "PM", "VC", "WS", "SM", "ST", "SA", "SN", "YU", "SC", "SL", "SG", "SK", "SI", "SB", "SO", "ZA", "GS", "ES", "LK", "SD", "SR", "SJ", "SZ", "SE", "CH", "SY", "TW", "TJ", "TZ", "TH", "TG", "TK", "TO", "TT", "TN", "TR", "TM", "TC", "TV", "UG", "UA", "AE", "GB", "US", "UM", "UY", "UZ", "VU", "VA", "VE", "VN", "VG", "VI", "WF", "EH", "YE", "CD", "ZM", "ZW" },
    new string[] { "Afghanistan", "Albania", "Algeria", "American Samoa", "Andorra", "Angola", "Anguilla", "Antarctica", "Antigua And Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bosnia And Herzegovina", "Botswana", "Bouvet Island", "Brazil", "British Indian Ocean Territory", "Brunei Darussalam", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Canada", "Cape Verde", "Cayman Islands", "Central African Republic", "Chad", "Chile", "China", "Christmas Island", "Cocos (keeling) Islands", "Colombia", "Comoros", "Congo", "Congo, The Democratic Republic Of The", "Cook Islands", "Costa Rica", "Côte D'ivoire", "Croatia", "Cuba", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "East Timor", "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Ethiopia", "Falkland Islands (malvinas)", "Faroe Islands", "Fiji", "Finland", "France", "French Guiana", "French Polynesia", "French Southern Territories", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Grenada", "Guadeloupe", "Guam", "Guatemala", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Heard Island And Mcdonald Islands", "Holy See (vatican City State)", "Honduras", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iran, Islamic Republic Of", "Iraq", "Ireland", "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakstan", "Kenya", "Kiribati", "Korea, Democratic Republic Of", "Korea, Republic Of", "Kuwait", "Kyrgyzstan", "Lao People's Democratic Republic", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libyan Arab Jamahiriya", "Liechtenstein", "Lithuania", "Luxembourg", "Macau", "Macedonia", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Martinique", "Mauritania", "Mauritius", "Mayotte", "Mexico", "Micronesia, Federated States Of", "Moldova, Republic Of", "Monaco", "Mongolia", "Montserrat", "Morocco", "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal", "Netherlands", "Netherlands Antilles", "New Caledonia", "New Zealand", "Nicaragua", "Niger", "Nigeria", "Niue", "Norfolk Island", "Northern Mariana Islands", "Norway", "Oman", "Pakistan", "Palau", "Palestinian Territory, Occupied", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Pitcairn", "Poland", "Portugal", "Puerto Rico", "Qatar", "Réunion", "Romania", "Russian Federation", "Rwanda", "Saint Helena", "Saint Kitts And Nevis", "Saint Lucia", "Saint Pierre And Miquelon", "Saint Vincent And The Grenadines", "Samoa", "San Marino", "Sao Tome And Principe", "Saudi Arabia", "Senegal", "Serbia and Montenegro", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa", "South Georgia, Sandwich Islands", "Spain", "Sri Lanka", "Sudan", "Suriname", "Svalbard And Jan Mayen", "Swaziland", "Sweden", "Switzerland", "Syrian Arab Republic", "Taiwan, Province Of China", "Tajikistan", "Tanzania, United Republic Of", "Thailand", "Togo", "Tokelau", "Tonga", "Trinidad And Tobago", "Tunisia", "Turkey", "Turkmenistan", "Turks And Caicos Islands", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "United States", "United States Minor Outlying Islands", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City State", "Venezuela", "Viet Nam", "Virgin Islands, British", "Virgin Islands, U.S.", "Wallis And Futuna", "Western Sahara", "Yemen", "Zaire (Congo)", "Zambia", "Zimbabwe" }
    );

    protected static Dictionary<string, string> languages = Exo.Collections.Util.CreateDictionary(
        new string[] { "de", "en" },
        new string[] { "Deutsch", "English" }
        );

    protected static Dictionary<string, string> birthMonths = Exo.Collections.Util.CreateDictionary(
        new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" },
        new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }
        );

    protected static string[] birthDays = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };
    protected static string[] birthYears = new string[] { "2007", "2006", "2005", "2004", "2003", "2002", "2001", "2000", "1999", "1998", "1997", "1996", "1995", "1994", "1993", "1992", "1991", "1990", "1989", "1988", "1987", "1986", "1985", "1984", "1983", "1982", "1981", "1980", "1979", "1978", "1977", "1976", "1975", "1974", "1973", "1972", "1971", "1970", "1969", "1968", "1967", "1966", "1965", "1964", "1963", "1962", "1961", "1960", "1959", "1958", "1957", "1956", "1955", "1954", "1953", "1952", "1951", "1950", "1949", "1948", "1947", "1946", "1945", "1944", "1943", "1942", "1941", "1940", "1939", "1938", "1937", "1936", "1935", "1934", "1933", "1932", "1931", "1930", "1929", "1928", "1927", "1926", "1925", "1924", "1923", "1922", "1921", "1920", "1919", "1918", "1917", "1916", "1915", "1914", "1913", "1912", "1911", "1910", "1909", "1908", "1907", "1906", "1905", "1904", "1903", "1902", "1901", "1900" };

    private UserFormMode mode;
    private UserManager userManager;
    private UserInfo user;
    private bool success;

    public UserForm()
    {
        Load += UserForm_Load;
    }

    private void UserForm_Load(object sender, EventArgs e)
    {
        if (Mode == UserFormMode.Edit)
        {
            UserNamePropertyProxyValidator.Enabled = false;
            UserNameCustomValidator.Enabled = false;
            PasswordPropertyProxyValidator.Enabled = false;
            RetypePasswordRequiredFieldValidator.Enabled = false;
        }

        if (Page.IsPostBack)
        {
            Page.Validate();

            if (Page.IsValid)
            {
                user.Email = this.email.Value;
                user.Username = this.userName.Value;

                if (!String.IsNullOrEmpty(this.password.Value))
                    user.Password = this.password.Value;

                user.FirstName = this.firstName.Value;
                user.LastName = this.lastName.Value;
                user.Gender = (UserInfo.UserGender)Byte.Parse(this.gender.SelectedItem.Value);
                user.Dob = new DateTime
                    (
                    Int32.Parse(this.birthYear.SelectedValue),
                    Int32.Parse(this.birthMonth.SelectedValue),
                    Int32.Parse(this.birthDay.SelectedValue)
                    );
                user.Location = this.location.SelectedValue;
                user.FavoritesPublic = favoritesPublic.Checked;
                user.HistoryPublic = historyPublic.Checked;
                user.RatedPublic = ratedPublic.Checked;

                UserManager.Save(user);

                userForm.Visible = false;
                success = true;

                if (Mode == UserFormMode.Create)
                {
                    Helper.SignIn(user.Id, user.Username);
                }
            }
        }
        else
            LoadDataSources();
    }

    private void LoadDataSources()
    {
        if (Request.UrlReferrer != null && Request.UrlReferrer.Host == Request.ServerVariables["HTTP_HOST"])
            returnUrl.Value = Request.Headers["Referer"];

        email.Value = user.Email;
        firstName.Value = user.FirstName;
        lastName.Value = user.LastName;
        gender.FindAndSelectByValue((byte)user.Gender);

        birthMonth.DataSource = birthMonths;
        birthMonth.DataBind();
        birthMonth.Items.Insert(0, new ListItem("Month", "-1"));
        
        birthDay.DataSource = birthDays;
        birthDay.DataBind();
        birthDay.Items.Insert(0, new ListItem("Day", "-1"));
        
        birthYear.DataSource = birthYears;
        birthYear.DataBind();
        birthYear.Items.Insert(0, new ListItem("Year", "-1"));

        if (user.Dob != default(DateTime))
        {
            birthMonth.FindAndSelectByValue(user.Dob.Month);
            birthDay.FindAndSelectByValue(user.Dob.Day);
            birthYear.FindAndSelectByValue(user.Dob.Year);
        }

        location.DataSource = locations;
        location.DataBind();
        location.Items.Insert(0, new ListItem("[Select Location]", "-1"));

        if (user.Location != null)
            location.FindAndSelectByValue(user.Location);

        else
            location.FindAndSelectByValue("United States");

        if (mode == UserFormMode.Edit)
        {
            favoritesPublic.Checked = user.FavoritesPublic;
            historyPublic.Checked = user.HistoryPublic;
            ratedPublic.Checked = user.RatedPublic;
        }
    }

    protected void ValidateEmail(object source, ServerValidateEventArgs args)
    {
        args.IsValid = (Mode == UserFormMode.Edit && args.Value == user.Email) || !UserManager.CheckEmailExists(args.Value);
    }

    protected void ValidateUserName(object source, ServerValidateEventArgs args)
    {
        args.IsValid = !UserManager.CheckUsernameExists(args.Value);
    }

    protected void ValidateBirthDate(object source, ServerValidateEventArgs args)
    {
        args.IsValid =
            birthMonths.ContainsKey(birthMonth.SelectedValue)
            && birthDay.SelectedIndex > 0 && birthDay.SelectedIndex - 1 < birthDays.Length
            && birthYear.SelectedIndex > 0 && birthYear.SelectedIndex - 1 < birthYears.Length;
    }

    protected void ValidateLocation(object source, ServerValidateEventArgs args)
    {
        args.IsValid = locations.ContainsKey(args.Value);
    }

    /*
    protected void ValidateCaptcha(object source, ServerValidateEventArgs args)
    {
        args.IsValid = captcha.UserValidated;
    }
    */

    public UserFormMode Mode
    {
        get { return mode; }
        set { mode = value; }
    }

    private UserManager UserManager
    {
        get { return userManager ?? (userManager = new UserManager(((CommonPage)Page).DatabaseConnection)); }
    }

    public UserInfo User
    {
        get { return user; }
        set { user = value; }
    }

    /// <summary>
    /// Indicates if the Create or Edit operation was successful
    /// </summary>
    public bool Success
    {
        get { return success; }
        set { success = value; }
    }

    public string ReturnUrl
    {
        get { return returnUrl.Value; }
    }
}

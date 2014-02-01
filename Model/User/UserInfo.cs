using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace Exo.Exoget.Model.User
{
    [HasSelfValidation]
    public class UserInfo
    {
        public enum UserGender : byte
        {
            Male,
            Female
        }

        private static readonly DateTime dobLower = DateTime.Parse("01-01-1900");
        private static readonly DateTime dobUpper = DateTime.Now;
        public static readonly Dictionary<string, string> locations;

        private uint id;
        private string username, password, email, firstName, lastName, location;
        private UserGender gender;
        private DateTime dob, lastActive;
        private bool favoritesPublic = true, historyPublic = true, ratedPublic = true;

        static UserInfo()
        {
            #region Locations

            locations = new Dictionary<string, string>();

            locations["AF"] = "Afghanistan";
            locations["AL"] = "Albania";
            locations["DZ"] = "Algeria";
            locations["AS"] = "American Samoa";
            locations["AD"] = "Andorra";
            locations["AO"] = "Angola";
            locations["AI"] = "Anguilla";
            locations["AQ"] = "Antarctica";
            locations["AG"] = "Antigua And Barbuda";
            locations["AR"] = "Argentina";
            locations["AM"] = "Armenia";
            locations["AW"] = "Aruba";
            locations["AU"] = "Australia";
            locations["AT"] = "Austria";
            locations["AZ"] = "Azerbaijan";
            locations["BS"] = "Bahamas";
            locations["BH"] = "Bahrain";
            locations["BD"] = "Bangladesh";
            locations["BB"] = "Barbados";
            locations["BY"] = "Belarus";
            locations["BE"] = "Belgium";
            locations["BZ"] = "Belize";
            locations["BJ"] = "Benin";
            locations["BM"] = "Bermuda";
            locations["BT"] = "Bhutan";
            locations["BO"] = "Bolivia";
            locations["BA"] = "Bosnia And Herzegovina";
            locations["BW"] = "Botswana";
            locations["BV"] = "Bouvet Island";
            locations["BR"] = "Brazil";
            locations["IO"] = "British Indian Ocean Territory";
            locations["BN"] = "Brunei Darussalam";
            locations["BG"] = "Bulgaria";
            locations["BF"] = "Burkina Faso";
            locations["BI"] = "Burundi";
            locations["KH"] = "Cambodia";
            locations["CM"] = "Cameroon";
            locations["CA"] = "Canada";
            locations["CV"] = "Cape Verde";
            locations["KY"] = "Cayman Islands";
            locations["CF"] = "Central African Republic";
            locations["TD"] = "Chad";
            locations["CL"] = "Chile";
            locations["CN"] = "China";
            locations["CX"] = "Christmas Island";
            locations["CC"] = "Cocos (keeling) Islands";
            locations["CO"] = "Colombia";
            locations["KM"] = "Comoros";
            locations["CG"] = "Congo";
            locations["CD"] = "Zaire (Congo)";
            locations["CK"] = "Cook Islands";
            locations["CR"] = "Costa Rica";
            locations["CI"] = "Côte D'ivoire";
            locations["HR"] = "Croatia";
            locations["CU"] = "Cuba";
            locations["CY"] = "Cyprus";
            locations["CZ"] = "Czech Republic";
            locations["DK"] = "Denmark";
            locations["DJ"] = "Djibouti";
            locations["DM"] = "Dominica";
            locations["DO"] = "Dominican Republic";
            locations["TP"] = "East Timor";
            locations["EC"] = "Ecuador";
            locations["EG"] = "Egypt";
            locations["SV"] = "El Salvador";
            locations["GQ"] = "Equatorial Guinea";
            locations["ER"] = "Eritrea";
            locations["EE"] = "Estonia";
            locations["ET"] = "Ethiopia";
            locations["FK"] = "Falkland Islands (malvinas)";
            locations["FO"] = "Faroe Islands";
            locations["FJ"] = "Fiji";
            locations["FI"] = "Finland";
            locations["FR"] = "France";
            locations["GF"] = "French Guiana";
            locations["PF"] = "French Polynesia";
            locations["TF"] = "French Southern Territories";
            locations["GA"] = "Gabon";
            locations["GM"] = "Gambia";
            locations["GE"] = "Georgia";
            locations["DE"] = "Germany";
            locations["GH"] = "Ghana";
            locations["GI"] = "Gibraltar";
            locations["GR"] = "Greece";
            locations["GL"] = "Greenland";
            locations["GD"] = "Grenada";
            locations["GP"] = "Guadeloupe";
            locations["GU"] = "Guam";
            locations["GT"] = "Guatemala";
            locations["GN"] = "Guinea";
            locations["GW"] = "Guinea-Bissau";
            locations["GY"] = "Guyana";
            locations["HT"] = "Haiti";
            locations["HM"] = "Heard Island And Mcdonald Islands";
            locations["VA"] = "Vatican City State";
            locations["HN"] = "Honduras";
            locations["HK"] = "Hong Kong";
            locations["HU"] = "Hungary";
            locations["IS"] = "Iceland";
            locations["IN"] = "India";
            locations["ID"] = "Indonesia";
            locations["IR"] = "Iran, Islamic Republic Of";
            locations["IQ"] = "Iraq";
            locations["IE"] = "Ireland";
            locations["IL"] = "Israel";
            locations["IT"] = "Italy";
            locations["JM"] = "Jamaica";
            locations["JP"] = "Japan";
            locations["JO"] = "Jordan";
            locations["KZ"] = "Kazakstan";
            locations["KE"] = "Kenya";
            locations["KI"] = "Kiribati";
            locations["KP"] = "Korea, Democratic Republic Of";
            locations["KR"] = "Korea, Republic Of";
            locations["KW"] = "Kuwait";
            locations["KG"] = "Kyrgyzstan";
            locations["LA"] = "Lao People's Democratic Republic";
            locations["LV"] = "Latvia";
            locations["LB"] = "Lebanon";
            locations["LS"] = "Lesotho";
            locations["LR"] = "Liberia";
            locations["LY"] = "Libyan Arab Jamahiriya";
            locations["LI"] = "Liechtenstein";
            locations["LT"] = "Lithuania";
            locations["LU"] = "Luxembourg";
            locations["MO"] = "Macau";
            locations["MK"] = "Macedonia";
            locations["MG"] = "Madagascar";
            locations["MW"] = "Malawi";
            locations["MY"] = "Malaysia";
            locations["MV"] = "Maldives";
            locations["ML"] = "Mali";
            locations["MT"] = "Malta";
            locations["MH"] = "Marshall Islands";
            locations["MQ"] = "Martinique";
            locations["MR"] = "Mauritania";
            locations["MU"] = "Mauritius";
            locations["YT"] = "Mayotte";
            locations["MX"] = "Mexico";
            locations["FM"] = "Micronesia, Federated States Of";
            locations["MD"] = "Moldova, Republic Of";
            locations["MC"] = "Monaco";
            locations["MN"] = "Mongolia";
            locations["MS"] = "Montserrat";
            locations["MA"] = "Morocco";
            locations["MZ"] = "Mozambique";
            locations["MM"] = "Myanmar";
            locations["NA"] = "Namibia";
            locations["NR"] = "Nauru";
            locations["NP"] = "Nepal";
            locations["NL"] = "Netherlands";
            locations["AN"] = "Netherlands Antilles";
            locations["NC"] = "New Caledonia";
            locations["NZ"] = "New Zealand";
            locations["NI"] = "Nicaragua";
            locations["NE"] = "Niger";
            locations["NG"] = "Nigeria";
            locations["NU"] = "Niue";
            locations["NF"] = "Norfolk Island";
            locations["MP"] = "Northern Mariana Islands";
            locations["NO"] = "Norway";
            locations["OM"] = "Oman";
            locations["PK"] = "Pakistan";
            locations["PW"] = "Palau";
            locations["PS"] = "Palestinian Territory, Occupied";
            locations["PA"] = "Panama";
            locations["PG"] = "Papua New Guinea";
            locations["PY"] = "Paraguay";
            locations["PE"] = "Peru";
            locations["PH"] = "Philippines";
            locations["PN"] = "Pitcairn";
            locations["PL"] = "Poland";
            locations["PT"] = "Portugal";
            locations["PR"] = "Puerto Rico";
            locations["QA"] = "Qatar";
            locations["RE"] = "Réunion";
            locations["RO"] = "Romania";
            locations["RU"] = "Russian Federation";
            locations["RW"] = "Rwanda";
            locations["SH"] = "Saint Helena";
            locations["KN"] = "Saint Kitts And Nevis";
            locations["LC"] = "Saint Lucia";
            locations["PM"] = "Saint Pierre And Miquelon";
            locations["VC"] = "Saint Vincent And The Grenadines";
            locations["WS"] = "Samoa";
            locations["SM"] = "San Marino";
            locations["ST"] = "Sao Tome And Principe";
            locations["SA"] = "Saudi Arabia";
            locations["SN"] = "Senegal";
            locations["YU"] = "Serbia and Montenegro";
            locations["SC"] = "Seychelles";
            locations["SL"] = "Sierra Leone";
            locations["SG"] = "Singapore";
            locations["SK"] = "Slovakia";
            locations["SI"] = "Slovenia";
            locations["SB"] = "Solomon Islands";
            locations["SO"] = "Somalia";
            locations["ZA"] = "South Africa";
            locations["GS"] = "South Georgia, Sandwich Islands";
            locations["ES"] = "Spain";
            locations["LK"] = "Sri Lanka";
            locations["SD"] = "Sudan";
            locations["SR"] = "Suriname";
            locations["SJ"] = "Svalbard And Jan Mayen";
            locations["SZ"] = "Swaziland";
            locations["SE"] = "Sweden";
            locations["CH"] = "Switzerland";
            locations["SY"] = "Syrian Arab Republic";
            locations["TW"] = "Taiwan, Province Of China";
            locations["TJ"] = "Tajikistan";
            locations["TZ"] = "Tanzania, United Republic Of";
            locations["TH"] = "Thailand";
            locations["TG"] = "Togo";
            locations["TK"] = "Tokelau";
            locations["TO"] = "Tonga";
            locations["TT"] = "Trinidad And Tobago";
            locations["TN"] = "Tunisia";
            locations["TR"] = "Turkey";
            locations["TM"] = "Turkmenistan";
            locations["TC"] = "Turks And Caicos Islands";
            locations["TV"] = "Tuvalu";
            locations["UG"] = "Uganda";
            locations["UA"] = "Ukraine";
            locations["AE"] = "United Arab Emirates";
            locations["GB"] = "United Kingdom";
            locations["US"] = "United States";
            locations["UM"] = "United States Minor Outlying Islands";
            locations["UY"] = "Uruguay";
            locations["UZ"] = "Uzbekistan";
            locations["VU"] = "Vanuatu";
            locations["VE"] = "Venezuela";
            locations["VN"] = "Viet Nam";
            locations["VG"] = "Virgin Islands, British";
            locations["VI"] = "Virgin Islands, U.S.";
            locations["WF"] = "Wallis And Futuna";
            locations["EH"] = "Western Sahara";
            locations["YE"] = "Yemen";
            locations["ZM"] = "Zambia";
            locations["ZW"] = "Zimbabwe";
            #endregion
        }

        public UserInfo(uint id)
        {
            this.id = id;
        }

        public UserInfo()
        {
            this.id = 0;
        }

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }

        [StringLengthValidator(1, 30, MessageTemplate = "Please enter a username between 1-30 characters in length")]
        [RegexValidator(@"^([A-Z0-9]+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase, MessageTemplate = "Sorry, A username can only contain letters A-Z and numbers 0-9. Please try again.")]
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        [StringLengthValidator(4, 20, MessageTemplate = "Please enter a password between 4-20 characters in length")]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        [StringLengthValidator(1, 100, MessageTemplate = "Please enter an email address.")]
        [RegexValidator(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", MessageTemplate = "Please enter a working email address in standard email format, e.g: joe@example.com.")]
        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        [StringLengthValidator(1, 20, MessageTemplate = "Please enter your first name")]
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        [StringLengthValidator(1, 20, MessageTemplate = "Please enter your last name")]
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

        [DateTimeRangeValidator("1900-01-01T00:00:00", "2007-01-01T00:00:00", MessageTemplate = "Please select a correct dob")]
        public DateTime Dob
        {
            get { return dob; }
            set { dob = value; }
        }

        public UserGender Gender
        {
            get { return gender; }
            set { gender = value; }
        }

        public DateTime LastActive
        {
            get { return lastActive; }
            set { lastActive = value; }
        }

        public string Location
        {
            get { return location; }
            set { location = value; }
        }

        public bool FavoritesPublic
        {
            get { return favoritesPublic; }
            set { favoritesPublic = value; }
        }

        public bool HistoryPublic
        {
            get { return historyPublic; }
            set { historyPublic = value; }
        }

        public bool RatedPublic
        {
            get { return ratedPublic; }
            set { ratedPublic = value; }
        }
    }
}

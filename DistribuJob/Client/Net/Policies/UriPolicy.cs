using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace DistribuJob.Client.Net.Policies
{
    [Serializable]
    public class UriPolicy
    {
        public const uint DynamicPolicyId = 0;

        public enum UriPolicyType : byte
        {
            ALLOW = 1,
            DISALLOW = 2,
            LINKS_NONE = 3,
            LINKS_MEDIA_ONLY = 4,
            LINE_HASH = 5,
            TITLE_SEGMENT_HASH = 6,
            TIMEOUT = 7,
            LINKS_MAX = 8,
            LINKS_INTERNAL_ONLY = 9,
            LINKS_EXTERNAL_ONLY = 10,
            LINKS_PICTURES_ONLY = 11,
            LINKS_PICTURES_NONE = 12,
            LINKS_DYNAMIC_ONLY = 13,
            LINKS_DYNAMIC_NONE = 14,
            LINKS_TEXT_MATCHES = 15,
            LINKS_DISALLOW_URI_MATCHES = 16,
            LINKS_PICTURE_URI_MATCHES = 17,
            LINE_META_INFO = 18,
            LINE_META_TITLE_INFO = 22,
            LINE_META_DESCRIPTION_INFO = 23,
            LINE_META_DURATION_INFO = 24,
            TITLE_SEGMENT_EXLCUDE_INDICIES = 19,
            LINE_LIMIT_HASH = 20,
            LINE_LIMIT_INDICE = 21,
            DOM_META_TITLE_XPATH = 25,
            DOM_META_DESCRIPTION_XPATH = 26,
            DOM_META_TAGS_XPATH = 27,
            DOM_META_AUTHOR_XPATH = 28,
            DOM_META_TARGET_XPATH = 29,
            DOM_META_DURATION_XPATH = 30,
            URI_VALUE_FORMAT_TARGET = 31,
            URI_CONTENT_FORMAT = 32

            /*
             * create way of extracting metadata either:
             * 
             * 1) Just one ArtificialMedia on a page, focused on a specific area (by titleSegmentUriPolicy or from centre 
             *    outwards + high absolute descriptivity).
             * 
             * 2) Each ArtificialMedia object or link to ArtificialMedia (text link + picture link  or  in table format with 
             *    high absolute descriptivity column).
             */
        }

        public readonly uint id;
        public readonly UriPolicyType type;
        public readonly Regex uriRegex;
        private DateTime expire;
        private object value;
        [NonSerialized]
        private int[] intArray;

        public UriPolicy(uint id, UriPolicyType type, Regex uriRegex)
        {
            this.id = id;
            this.type = type;
            this.uriRegex = uriRegex;
            this.expire = DateTime.MinValue;
        }

        public UriPolicy(uint id, UriPolicyType type, string regex)
            : this(id, type, new Regex(regex))
        {
        }

        public UriPolicy()
        {
        }

        public bool MatchesUri(string uri)
        {
            return uriRegex == null || uriRegex.IsMatch(uri);
        }

        public bool MatchesUri(Uri uri)
        {
            return MatchesUri(uri.ToString());
        }

        public void UpdateValue()
        {
            Dj.Djs.SetUriPolicyValue(id, value);
        }

        public bool IsDynamicPolicy
        {
            get { return id == DynamicPolicyId; }
        }

        public DateTime Expire
        {
            get { return expire; }
            set { this.expire = value; }
        }

        public bool IsExpired
        {
            get { return expire != null && expire != DateTime.MinValue && expire >= DateTime.Now; }
        }

        #region Value Properties

        public object Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public bool HasValue
        {
            get { return value != null; }
        }

        public string StringValue
        {
            get { return (string)value; }
            set { this.value = value; }
        }

        public int IntValue
        {
            get { return Convert.ToInt32(value); }
            set { this.value = value; }
        }

        public int[] IntArrayValue
        {
            get
            {
                if (intArray == null)
                {
                    string[] segments = StringValue.Split(',');
                    intArray = new int[segments.Length];

                    for (int i = 0; i < intArray.Length; i++)
                        intArray[i] = Int32.Parse(segments[i]);
                }

                return intArray;
            }

            set
            {
                intArray = value;

                StringBuilder strIntArray = new StringBuilder(intArray.Length * 12);

                foreach (int i in intArray)
                    strIntArray.Append(i + ",");

                strIntArray.Remove(strIntArray.Length - 1, 1);

                StringValue = strIntArray.ToString();
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exo.Exoget.Model.Search;

namespace Exo.Exoget.Model
{
    public abstract class PropertiesBase
    {
        internal IndexPropertyInfo[] properties;

        public IndexPropertyInfo[] GetPropertiesByType(IndexPropertyType type)
        {
            return (from property in Properties where property.Type == type select property).ToArray<IndexPropertyInfo>();
        }

        public bool TryGetSinglePropertyByType(IndexPropertyType type, out IndexPropertyInfo property)
        {
            IndexPropertyInfo[] props = GetPropertiesByType(type);

            if (props.Length > 0)
            {
                property = props[0];
                return true;
            }
            else
            {
                property = null;
                return false;
            }
        }

        public IndexPropertyInfo GetSinglePropertyByType(IndexPropertyType type)
        {
            IndexPropertyInfo[] props = GetPropertiesByType(type);

            if (props.Length > 0)
                return props[0];

            else
                return null;
        }

        public string GetSinglePropertyValueByType(IndexPropertyType type)
        {
            IndexPropertyInfo property;

            return TryGetSinglePropertyByType(type, out property) ? property.Value : null;
        }

        public IndexPropertyInfo[] Properties
        {
            get { return properties ?? new IndexPropertyInfo[0]; }
            set { properties = value; }
        }

        #region Properties

        public IndexPropertyInfo Author
        {
            get { return GetSinglePropertyByType(IndexPropertyType.Author); }
        }

        public string[] Authors
        {
            get { return Author == null ? new string[0] : Author.Value.Split(','); }
        }

        public IndexPropertyInfo Album
        {
            get { return GetSinglePropertyByType(IndexPropertyType.Album); }
        }

        public IndexPropertyInfo Genre
        {
            get { return GetSinglePropertyByType(IndexPropertyType.Genre); }
        }

        public IndexPropertyInfo[] Keywords
        {
            get { return GetPropertiesByType(IndexPropertyType.Keyword); }
        }

        public IndexPropertyInfo Year
        {
            get { return GetSinglePropertyByType(IndexPropertyType.Year); }
        }

        public DateTime Pubdate
        {
            get
            {
                string pubdateStr = GetSinglePropertyValueByType(IndexPropertyType.Pubdate);

                if (pubdateStr != null)
                    return new DateTime(Int64.Parse(pubdateStr));

                else
                    return default(DateTime);
            }
        }

        public bool HasPubdate
        {
            get { return Pubdate != default(DateTime); }
        }
        #endregion
    }
}

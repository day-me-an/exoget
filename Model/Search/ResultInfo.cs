using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using Exo.Exoget.Model.Media;
using System.Text.RegularExpressions;

namespace Exo.Exoget.Model.Search
{
    public class ResultInfo
    {
        private ICollection<MediaInfo> medias;
        //private string[] querySuggestions;
        private uint resultsFoundCount;
        private Regex tokenRegex;
        //private Dictionary<IndexPropertyType, List<RelatedProperty>> relatedProperties;

        public ResultInfo()
        {
        }

        /// <summary>
        /// Gets all properties of a specific type from all of the items contained within this result
        /// </summary>
        public IndexPropertyInfo[] GetPropertiesByType(IndexPropertyType type)
        {
            List<IndexPropertyInfo> mediaPropertiesList = new List<IndexPropertyInfo>();

            foreach (MediaInfo media in medias)
                mediaPropertiesList.AddRange(media.GetPropertiesByType(type));

            return mediaPropertiesList.ToArray();
        }

        /// <summary>
        /// The media items returned by the operation that created this result
        /// </summary>
        public ICollection<MediaInfo> Medias
        {
            get { return medias; }
            set { medias = value; }
        }

        /// <summary>
        /// Alternative forms a query for when a spelling mistake was determined to be in the original
        /// </summary>
        /*public string[] QuerySuggestions
        {
            get { return querySuggestions; }
            set { querySuggestions = value; }
        }*/

        /// <summary>
        /// The number of items contained within this result
        /// </summary>
        public int ResultsCount
        {
            get { return Medias != null ? Medias.Count : 0; }
        }

        /// <summary>
        /// The total number of items found by the operation that created this result
        /// </summary>
        public uint ResultsFoundCount
        {
            get { return resultsFoundCount; }
            set { resultsFoundCount = value; }
        }

        /// <summary>
        /// A Regex of the terms to highlight when displaying the items in this result
        /// </summary>
        public Regex TokenRegex
        {
            get { return tokenRegex; }
            set { tokenRegex = value; }
        }

        /*
        public Dictionary<IndexPropertyType, List<RelatedProperty>> RelatedProperties
        {
            get { return relatedProperties ?? (relatedProperties = new Dictionary<IndexPropertyType, List<RelatedProperty>>()); }
        }

        public void AddRelatedProperty(IndexPropertyType type, RelatedProperty relatedProperty)
        {
            List<RelatedProperty> relatedProps;

            if (!RelatedProperties.TryGetValue(type, out relatedProps))
                relatedProperties[type] = new List<RelatedProperty>(6);

            relatedProperties[type].Add(relatedProperty);
        }
        */ 
    }
}
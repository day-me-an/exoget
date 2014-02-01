using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exo.Exoget.Model.Search;

namespace DistribuJob.Client.Extracts
{
    [Serializable]
    public abstract class IndexPropertiesBase
    {
        [NonSerialized]
        private List<IndexPropertyInfo> indexProperties;

        public abstract void AddIndexProperties();

        protected bool TryAddIndexProperty(IndexPropertyType type, string value)
        {
            if (type > 0 && !String.IsNullOrEmpty(value))
            {
                IndexPropertiesList.Add(new IndexPropertyInfo(type, value));

                return true;
            }
            else
                return false;
        }

        protected bool TryAddIndexProperty(IndexPropertyType type, DateTime value)
        {
            if (value != default(DateTime))
                return TryAddIndexProperty(type, value.Ticks.ToString());

            else
                return false;
        }

        protected bool TryAddIndexProperty(IndexPropertyType type, IEnumerable<string> values)
        {
            if (values != null)
            {
                bool success = true;

                foreach (string value in values)
                {
                    if (!TryAddIndexProperty(type, value) && success)
                        success = false;
                }

                return success;
            }
            else
                return false;
        }

        protected List<IndexPropertyInfo> IndexPropertiesList
        {
            get { return indexProperties ?? (indexProperties = new List<IndexPropertyInfo>()); }
        }

        public IEnumerable<IndexPropertyInfo> IndexProperties
        {
            get
            {
                if (indexProperties == null)
                    AddIndexProperties();

                return IndexPropertiesList;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Exo.Exoget.Model.Search
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class IndexWordAttribute : Attribute
    {
        private readonly string searchFieldName;

        public IndexWordAttribute(string searchFieldName)
        {
            this.searchFieldName = searchFieldName;
        }

        public string SearchFieldName
        {
            get { return searchFieldName; }
        } 
    }
}

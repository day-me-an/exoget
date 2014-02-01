using System;
using System.Collections.Generic;
using System.Text;

namespace Exo.Exoget.Model.Search
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class SearchModiferWordAttribute : Attribute
    {
        private readonly string word;

        public SearchModiferWordAttribute(string word)
        {
            this.word = word;
        }

        public string Word
        {
            get { return word; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Exo.Exoget.Model.Search;

namespace DistribuJob.Client.Extracts
{
    public interface IIndexProperties
    {
        public IEnumerable<IndexPropertyInfo> IndexProperties
        {
            get;
        }
    }
}

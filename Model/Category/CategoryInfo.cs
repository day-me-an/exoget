using System;
using System.Collections.Generic;
using System.Text;

namespace Exo.Exoget.Model.Category
{
    public class CategoryInfo
    {
        private readonly uint id;
        private uint itemsCount;
        private string title;
        private CategoryInfo parent;
        private List<CategoryInfo> children;

        public uint Id
        {
            get { return id; }
        }

        public uint ItemsCount
        {
            get { return itemsCount; }
            set { itemsCount = value; }
        }
    }
}

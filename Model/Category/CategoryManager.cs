using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace Exo.Exoget.Model.Category
{
    public class CategoryManager
    {
        private readonly MySqlConnection conn;

        public CategoryManager(MySqlConnection conn)
        {
            this.conn = conn;
        }
        /*
        public CategoryInfo[] AllCategories
        {
            get
            {
                Dictionary<uint?, CategoryInfo> categoryChildren = new Dictionary<uint?, CategoryInfo>();
                uint noParent = 0;

                using (MySqlCommand command = new MySqlCommand("SELECT ids, auth, parent, title FROM categories ORDER BY parent, title", conn))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CategoryInfo category = new CategoryInfo();

                        if (reader.IsDBNull(2))
                            category.parent = null;

                        else
                            category.parent = reader.GetUInt32(2);

                        category.Id = reader.GetUInt32(0);
                        category.Auth = reader.GetInt32(1);
                        category.Title = reader.GetString(3);

                        if (category.parent != null)
                            categoryChildren[category.parent].childs.Add(category);

                        else
                            noParent++;

                        categoryChildren[category.ids] = category;
                    }
                }

                CategoryInfo[] categories = new CategoryInfo[noParent];

                int read = 0;
                foreach (CategoryInfo category in categoryChildren.Values)
                {
                    if (!category.HasParent)
                    {
                        categories[read++] = category;
                    }
                }

                return categories;
            }
        }
        */
    }
}

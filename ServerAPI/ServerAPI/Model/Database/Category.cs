using System;
using System.Collections.Generic;

namespace ServerAPI.Model.Database
{
    public partial class Category
    {
        public Category()
        {
            CategoryItems = new HashSet<CategoryItem>();
        }

        public int? Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<CategoryItem> CategoryItems { get; set; }
    }
}

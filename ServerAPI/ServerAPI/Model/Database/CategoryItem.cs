﻿using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class CategoryItem
    {
        public int? Id { get; set; }
        public string CategoryItemName { get; set; }
        public int CategoryId { get; set; }
        public int UnitPrice { get; set; }
        public string ImageUrl { get; set; }

        public virtual Category Category { get; set; }
    }
}

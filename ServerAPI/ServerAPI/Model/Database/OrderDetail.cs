using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class OrderDetail
    {
        public int? Id { get; set; }
        public int CategoryItemId { get; set; }
        public int Amount { get; set; }
        public int OrderId { get; set; }

        public virtual CategoryItem CategoryItem { get; set; }
        public virtual Order Order { get; set; }
    }
}

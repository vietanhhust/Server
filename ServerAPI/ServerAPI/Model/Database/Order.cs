using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int? Id { get; set; }
        public int? AccountId { get; set; }
        public int? AdminId { get; set; }
        public long CreatedTime { get; set; }
        public bool Status { get; set; }
        public int? ClientId { get; set; }

        public virtual Account Account { get; set; }
        public virtual ManagingAccount Admin { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

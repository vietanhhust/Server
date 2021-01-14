using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class ManagingAccount
    {
        public ManagingAccount()
        {
            HistoryChangeBalances = new HashSet<HistoryChangeBalance>();
            Orders = new HashSet<Order>();
        }

        public int? Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int? GroupRoleId { get; set; }
        public string Description { get; set; }

        public virtual GroupRole GroupRole { get; set; }
        public virtual ICollection<HistoryChangeBalance> HistoryChangeBalances { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}

using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class HistoryChangeBalance
    {
        public int? Id { get; set; }
        public int ManagingAccountId { get; set; }
        public int AccountId { get; set; }
        public int Cost { get; set; }
        public bool TypeChange { get; set; }
        public long? TimeChange { get; set; }

       public virtual Account Account { get; set; }
       public virtual ManagingAccount ManagingAccount { get; set; }
    }
}

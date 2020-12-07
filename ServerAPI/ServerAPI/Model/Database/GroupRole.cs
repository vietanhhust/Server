using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class GroupRole
    {
        public GroupRole()
        {
            ManagingAccounts = new HashSet<ManagingAccount>();
            RoleActives = new HashSet<RoleActive>();
        }

        public int? Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<ManagingAccount> ManagingAccounts { get; set; }
        public virtual ICollection<RoleActive> RoleActives { get; set; }
    }
}

using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class RoleActive
    {
        public int Id { get; set; }
        public int GroupRoleId { get; set; }
        public int RoleId { get; set; }
        public bool Active { get; set; }

        public virtual GroupRole GroupRole { get; set; }
        public virtual Role Role { get; set; }
    }
}

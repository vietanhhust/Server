using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class ManagingAccount
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int? GroupRoleId { get; set; }
        public string Description { get; set; }

        public virtual GroupRole GroupRole { get; set; }
    }
}

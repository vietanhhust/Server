using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class RoleActive
    {
        public int? Id { get; set; }
        public int GroupRoleId { get; set; }
        public int? FrontendRoleId { get; set; }
        public bool? IsView { get; set; }
        public bool? IsCreate { get; set; }
        public bool? IsPut { get; set; }
        public bool? IsDelete { get; set; }
        public bool? isActive { get; set; }

        public virtual FrontendRole FrontendCodeNavigation { get; set; }
        public virtual GroupRole GroupRole { get; set; }
    }
}

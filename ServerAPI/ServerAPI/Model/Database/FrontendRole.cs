using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class FrontendRole
    {
        public FrontendRole()
        {
            RoleActives = new HashSet<RoleActive>();
        }

        public int? Id { get; set; }
        public int? FrontendCode { get; set; }
        public string Description { get; set; }

        public virtual ICollection<RoleActive> RoleActives { get; set; }
    }
}

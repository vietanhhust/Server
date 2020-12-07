using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class Role
    {
        public int? Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public string Template { get; set; }
        public int? FrontEndId { get; set; }
        public string Method { get; set; }
    }
}

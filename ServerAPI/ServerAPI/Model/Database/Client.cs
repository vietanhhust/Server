using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class Client
    {
        public int? Id { get; set; }
        public int ClientId { get; set; }
        public int? ClientGroupId { get; set; }
        public string Description { get; set; }
        public bool? IsNew { get; set; }

        public virtual GroupClient ClientNavigation { get; set; }
    }
}

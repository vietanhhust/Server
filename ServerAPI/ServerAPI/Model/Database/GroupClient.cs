using System;
using System.Collections.Generic;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class GroupClient
    {
        public GroupClient()
        {
            Clients = new HashSet<Client>();
        }

        public int? Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }

        public virtual ICollection<Client> Clients { get; set; }
    }
}

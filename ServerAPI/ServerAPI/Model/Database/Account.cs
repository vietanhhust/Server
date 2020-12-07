using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class Account
    {
        public int? Id { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public int? Balance { get; set; }
        public string Description { get; set; }
        public string IdentityNumber { get; set; }
        public int? Debit { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int? Credit { get; set; }
        public int? ElaspedTime { get; set; }
        public bool? IsActived { get; set; }
        public bool? IsLogged { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Model.Errors
{
    public class ErrorModel
    {
        public int? Status { get; set; } = 400;
        public string Messege { get; set; } 
    }
}

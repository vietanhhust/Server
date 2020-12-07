using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Model.StaticModel
{
    public static class StaticConsts
    {
        public static List<int> UndeletableGroupRole = new List<int>()
        {
            1, 2
        };

        public static List<int> UndeletableGroupClient = new List<int>()
        {
            1
        };

        
    }

    public enum MethodEnum
    {
        GET = 1,
        Post = 2,
        Put = 3,
        Delete = 4
    }

   
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper; 

namespace ServerAPI.Model.Mappings
{

    // Vì nếu k đăng ký create Map, thì khi dùng Mapper sẽ không map được 
    //vậy nên phải đăng kí map asembly trước đã. 
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            Type[] lst = this.GetTypesInNamespace(Assembly.GetAssembly(typeof(MappingProfile)), "ServerAPI.Model.Database");
            lst.ToList().ForEach(item =>
            {
                try
                {
                    Console.WriteLine(item.Name);
                    Console.WriteLine(item.FullName);
                    this.CreateMap(Type.GetType(item.FullName), Type.GetType(item.FullName)).ReverseMap().
                    ForAllMembers(opt=> opt.Condition((src,des, srcMember)=> srcMember != null));
                }
                catch
                {
                }
            });
        }

        // Hiện tại chỉ cần map đúng để thực hiện CRUD
        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace))
                      .ToArray();
        }
    }

   
}

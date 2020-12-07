using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ServerAPI.Model.Errors;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Database;

namespace ServerAPI.Utilities
{
    public class TestActionFilterAttribute : ActionFilterAttribute
    {
        private EntityCRUDService entityCRUD;
        public TestActionFilterAttribute(EntityCRUDService entityCRUD)
        {
            this.entityCRUD = entityCRUD; 
            Console.WriteLine("Xuất hiện: " + this.entityCRUD.GetAll<GroupRole>().Count);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine("After Excute: {0}", context.ActionDescriptor.AttributeRouteInfo.Template);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Request.Query.ToList().ForEach(item =>
            {
                Console.WriteLine(String.Format("Key:{0}, value: {1}",item.Key, item.Value));
            });
            Console.WriteLine("Before Excute: {0}", context.ActionDescriptor.AttributeRouteInfo.Template);
        }
    }
}

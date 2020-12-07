using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;

namespace ServerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteInspectController : ControllerBase
    {
        private IActionDescriptorCollectionProvider inspector;
        public RouteInspectController(IActionDescriptorCollectionProvider inspector)
        {
            this.inspector = inspector;
        }

        [HttpGet]
        [Route("all")]
        public IActionResult getAllRouter()
        {
            Console.WriteLine(this.HttpContext.Connection.RemoteIpAddress);
            var a = this.inspector.ActionDescriptors.Items.ToList();
            if(a is null)
            {
                return Ok("null");
            }
            List<dynamic> x = new List<dynamic>();
            a.ForEach(item =>
            {
                if(item.AttributeRouteInfo is null)
                {
                }
                else
                {
                    var metadataLength = item.EndpointMetadata.Count;
                    var lastItem = item.EndpointMetadata[metadataLength - 1];
                    var method = (lastItem as dynamic).HttpMethods;
                    x.Add(new {
                        template = item.AttributeRouteInfo.Template,
                        name = item.AttributeRouteInfo.Name,
                        displayName = item.DisplayName, 
                        method = method[0],
                        
                    });
                }
            });
            return Ok(JsonConvert.SerializeObject(x));
        }

        [Route("asm/{id}")]
        [HttpGet]
        public object getAssembly()
        {
            return this.ControllerContext.ActionDescriptor.AttributeRouteInfo.Template;
        }


        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;
using ServerAPI.Model.StaticModel;

namespace ServerAPI.Controllers.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncToolController : ControllerBase
    {
        IActionDescriptorCollectionProvider inspector; 
        public EntityCRUDService entityCRUD;
        public ClientManagerContext context;
        public SyncToolController(EntityCRUDService entityCRUD, ClientManagerContext context,
               IActionDescriptorCollectionProvider inspector
        )
        {
            this.entityCRUD = entityCRUD; ;
            this.context = context;
            this.inspector = inspector;
        }

        // Route này nhận request ghi lại bảng template URL trong cơ sở dữ liệu
        [Route("sync")]
        [HttpPost]
        [IgnoreFilterAtrribute]
        public async Task<IActionResult> Sync()
        {
            List<Role> roles = new List<Role>(); 
            // Những template cũ
            var oldTemplate = this.entityCRUD.GetAll<Role>().ToList();
            // Những template mới của project
            var newApi = this.getAllEndpoint();

            // Xóa sạch các template ở bảng cũ 
            var truncateResult = this.entityCRUD.ExcuteCommand(@"truncate table dbo.Role");
            Console.WriteLine("resulte: " + truncateResult);
            this.entityCRUD.UnTrackContext(); 
            // Nếu những template mới có tồn tại ở templat cũ thì giữ nguyên, mới thì thêm mới
            newApi.ToList().ForEach(item =>
            {
                var oldTemplateEntityFound = oldTemplate.
                    Where(x => x.Method.ToLower() == item.Method.ToLower() && x.Template.ToLower() == item.Template.ToLower()).FirstOrDefault();
                if(oldTemplateEntityFound != null)
                {
                    roles.Add(new Role
                    {
                        Id = null,
                        Description = oldTemplateEntityFound.Description,
                        FrontendCode = oldTemplateEntityFound.FrontendCode,
                        Method = oldTemplateEntityFound.Method,
                        Template = oldTemplateEntityFound.Template
                    });
                }
                else
                {
                    roles.Add(new Role
                    {
                        Id = null,
                        Description = "",
                        Template = item.Template.ToLower(),
                        FrontendCode = 0,
                        Method = item.Method.ToLower()
                    });
                }
            });

            if (this.entityCRUD.AddRange<Role>(roles).Result)
            {
                return await Task<IActionResult>.Factory.StartNew(() => Ok(true));
            }
            else { return await Task<IActionResult>.Factory.StartNew(() => BadRequest(false)); }

        }
        
        //Hàm này để lấy ra tât cả các endpoint của Application;
        private IList<APIDescriptionModel> getAllEndpoint()
        {
            List<APIDescriptionModel> listAPI = new List<APIDescriptionModel>();
            this.inspector.ActionDescriptors.Items.ToList().ForEach(item =>
            {
                if (item.AttributeRouteInfo is null)
                {

                }
                else
                {
                    var metadataLength = item.EndpointMetadata.Count;
                    var lastItem = item.EndpointMetadata[metadataLength - 1];
                    var method = (lastItem as dynamic).HttpMethods;
                    listAPI.Add(new APIDescriptionModel
                    {
                        Template = item.AttributeRouteInfo.Template,
                        FrontendCode = 0,
                        Method = method[0],

                    });
                }
            });
            listAPI = listAPI.OrderByDescending(item=> item.FrontendCode).ToList();
            return listAPI;
        }

        // Map 1 template Url vào FrontendId
        [HttpPut]
        [Route("mapTemplate/{id}")]
        public IActionResult mapTemplate(int id, [FromBody] List<APIDescriptionModel> listAPI) 
        {
            // id là frontend code
            // Tìm xem nhóm quyền front end có tồn tại k. 
            if(this.entityCRUD.GetAll<FrontendRole>(x=>x.FrontendCode==id).FirstOrDefault() == null)
            {
                return BadRequest(new ErrorModel { Messege = "Nhóm quyền không tồn tại" });
            }

            //Update role FrontEnd
            List<Role> rolesFound = new List<Role>();
            listAPI.ForEach(item =>
            {
                var roleFound = this.entityCRUD.GetAll<Role>(x =>
                    x.Method.ToLower() == item.Method.ToLower() &&
                    x.Template.ToLower() == item.Template.ToLower()).FirstOrDefault(); 
                if(rolesFound != null && (roleFound.FrontendCode != null || roleFound.FrontendCode == 0))
                {
                    roleFound.FrontendCode = id;
                    rolesFound.Add(roleFound);
                }
            });

            if (this.entityCRUD.UpdateRange<Role>(rolesFound).Result)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(new ErrorModel { Messege = "Có lỗi xảy ra" });
            }
        }

        // Xóa 1 api khỏi một nhóm quyền template 
        [HttpDelete]
        [Route("remove/{id}")]
        public IActionResult removeTemplate(int id, [FromBody] APIDescriptionModel api)
        {
            // Id là mã FrontEnd code. 
            var roleFound = this.entityCRUD.GetAll<Role>(x => x.Method.ToLower() == api.Method.ToLower()
                && x.Template.ToLower() == api.Template.ToLower()).FirstOrDefault();
            roleFound.FrontendCode = 0;
            if(this.entityCRUD.Update<Role, Role>(roleFound, roleFound).Result)
            {
                return Ok(id);
            }
            else {
                return BadRequest(new ErrorModel
                {
                    Messege = "Có lỗi xảy ra vui lòng thử lại"
                }); }
        }

        // Máy chủ sẽ có 1 biến global static, chứa danh sách các client ( dù có tham gia kết nối hay không)
        // Máy chủ cũng sẽ có 1 biến global static chứa  danh sách các adminPage đang tham gia kết nối. 
        // Gọi Api này để đồng bộ ( nếu có client mới được setup)
        [HttpPost]
        [Route("syncClient")]
        public IActionResult syncClient()
        {
            this.entityCRUD.GetAll<Client>().ToList().ForEach(item =>
            {
                var clientFound = StaticConsts.ConnectedClient.FirstOrDefault(x => x.ClientId == item.ClientId);
                if(clientFound is null)
                {
                    StaticConsts.ConnectedClient.Add(new ClientConnect { ClientId = item.ClientId, ConnectionId = "" });
                }
                else
                {

                }
            });
            return Ok(StaticConsts.ConnectedClient.Count);
        }
    }


    
    

}

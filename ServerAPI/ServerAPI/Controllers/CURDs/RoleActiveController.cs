using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleActiveController : ControllerBase
    {
        private EntityCRUDService entityCRUD;
        private ClientManagerContext context; 
        
        public RoleActiveController(EntityCRUDService entityCRUD, ClientManagerContext context)
        {
            this.entityCRUD = entityCRUD;
            this.context = context;
        }

        // Update role. 
        [HttpPut]
        [Route("roleActive/{id}")]
        public IActionResult updateRoleActive(int id, [FromBody] List<RoleActive> roles)
        {
            // Tìm nhóm quyền ứng với id
            // 1 là default, 2 là quản trị
            var groupRoleFound = this.entityCRUD.GetAll<GroupRole>(x => x.Id == id).FirstOrDefault(); 
            if(groupRoleFound.Id == 2)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Nhóm quyền không thể sửa"
                });
            }
            if(groupRoleFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Nhóm quyền không tồn tại"
                });
            }
            return BadRequest(); 
        }
    }
}

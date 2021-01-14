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

        // Get FrontendRoleActives by grouproleId
        [HttpGet]
        [Route("group/{id}")]
        public IActionResult getRoleActiveByGroup(int id, [FromQuery] string keyword = "")
        {
            var lstRoleActive = this.entityCRUD.GetAll<RoleActive>(x => x.GroupRoleId == id).ToList();
            var lstFrontEndRole = this.entityCRUD.GetAll<FrontendRole>().ToList();
            if(keyword is null) {
                keyword = "";
            }
            var x = (from roleActive in lstRoleActive
                    join frontEndRole in lstFrontEndRole
                    on roleActive.FrontendRoleId equals frontEndRole.Id
                    select new { 
                        roleActive.Id, 
                        groupRoleId = id,
                        frontendRoleId = frontEndRole.Id, 
                        isActive = roleActive.IsActive,
                        frontendRoleName = frontEndRole.Description, 
                        frontendCode = frontEndRole.FrontendCode
                    }).OrderByDescending(a=>a.frontendCode).ToList().Where(x=>x.frontendRoleName.ToLower().Contains(keyword.ToLower()));
            return Ok(x);
        }


        // Update role. 
        [HttpPut]
        [Route("group/{id}")]
        public IActionResult updateRoleActive(int id, [FromBody] List<RoleGrantModel> roles)
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

            var lstRole = this.entityCRUD.GetAll<RoleActive>(x => x.GroupRoleId == id).ToList();
            roles.ForEach(item =>
            {
                var rolesToFix = lstRole.Where(x => x.Id == item.Id).FirstOrDefault(); 
                if(rolesToFix is null)
                {

                }
                else
                {
                    rolesToFix.IsActive = item.isActive;
                }
            });

            if (this.entityCRUD.UpdateRange<RoleActive>(lstRole).Result)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}


public class RoleGrantModel
{
    public int FrontendCode { get; set; }
    public int FrontendRoleId {get; set; }
    public string FrontendRoleName { get; set; }
    public int GroupRoleId { get; }
    public int Id { get; set;  }
    public bool isActive { get; set; }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;
using ServerAPI.Model.StaticModel;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupRolesController : ControllerBase
    {
        private ClientManagerContext context;
        private EntityCRUDService entityCRUD;
        public GroupRolesController(ClientManagerContext context, EntityCRUDService entityCRUD)
        {
            this.context = context;
            this.entityCRUD = entityCRUD;
        }

        // Lấy danh sách nhóm quyền về
        [HttpGet]
        public IActionResult GetGroupRoles()
        {
            return Ok(this.entityCRUD.GetAll<GroupRole>().ToList());
        }

        // GET: api/GroupRoles/5
        [HttpGet("{id}")]
        public  IActionResult GetGroupRole(int id)
        {
            var groupRole = this.entityCRUD.GetAll<GroupRole>(x => x.Id == id).FirstOrDefault();
            if(groupRole is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Nhóm quyền không tồn tại"
                });
            }
            else
            {
                return Ok(groupRole);
            }
        }

        // Chỉnh sửa thông tin nhóm quyền
        [HttpPut("{id}")]
        public IActionResult PutGroupRole(int id, [FromBody] GroupRole groupRole)
        {
            groupRole.Id = id;
            var groupRoleFound = this.context.GroupRoles.FirstOrDefault(group => group.Id == id);
            if (groupRoleFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy nhóm quyền"
                });
            }
            else
            {
                if(this.entityCRUD.Update<GroupRole, GroupRole>(groupRole, groupRoleFound).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Vui lòng nhập đủ các trường"
                    });
                }
            }
        }

        // Thêm mới nhóm quyền
        [HttpPost]
        public IActionResult PostGroupRole([FromBody] GroupRole groupRole)
        {
            groupRole.Id = null;
            if (this.entityCRUD.Add<GroupRole>(groupRole).Result)
            {
                // Sau khi thêm mới, ghi vào bảng RoleActive những quyền ứng với frontend Id 
                var listFrontEndRole = this.entityCRUD.GetAll<FrontendRole>().ToList();
                var listNewRoleActive = new List<RoleActive>();
                listFrontEndRole.ForEach(item =>
                {
                    listNewRoleActive.Add(new RoleActive
                    {
                        FrontendRoleId = item.Id,
                        GroupRoleId = groupRole.Id.Value,
                        IsCreate = false,
                        IsDelete = false,
                        IsPut = false,
                        IsView = false
                    });
                });
                if (this.entityCRUD.AddRange<RoleActive>(listNewRoleActive).Result)
                {
                    return Ok(groupRole.Id);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Không thể thiêt lập nhóm quyền mới"
                    });
                }
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Vui lòng nhập đủ các trường"
                });
            }
        }

        //Xóa nhóm quyền
        [HttpDelete("{id}")]
        public IActionResult DeleteGroupRole(int id)
        {
            var groupRoleFound = this.entityCRUD.GetAll<GroupRole>(item => item.Id == id).FirstOrDefault();
            if (StaticConsts.UndeletableGroupRole.Contains(id))
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Nhóm quyền không thể xóa"
                });
            }
            if(groupRoleFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Nhóm quyền không tồn tại"
                });
            }
            if(this.context.ManagingAccounts.Where(item=>item.GroupRoleId==id).ToList().Count > 0)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Nhóm quyền này vẫn đang còn user"
                });
            }
            // Xóa những roleActive liên quan đến nhóm quyền 
            var listRoleActive = this.entityCRUD.GetAll<RoleActive>(x => x.GroupRoleId == id).ToList();
            if (this.entityCRUD.DeleteRange<RoleActive>(listRoleActive).Result)
            {
                if (this.entityCRUD.Delete<GroupRole>(groupRoleFound).Result)
                {
                    return Ok(id);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Có lỗi xảy ra vui lòng thử lại sau"
                    });
                }
            }
            else {
                return BadRequest(new ErrorModel
                {
                    Messege = "Có lỗi xảy ra vui lòng thử lại sau"
                });
            };


           
        }

        private bool GroupRoleExists(int id)
        {
            return context.GroupRoles.Any(e => e.Id == id);
        }
    }
}

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

// Phần này cần phải viêt lại, cho phần syncToolController
namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private ClientManagerContext context;
        private EntityCRUDService entityCRUD;
        public RolesController(ClientManagerContext context, EntityCRUDService entityCRUD)
        {
            this.entityCRUD = entityCRUD;
            this.context = context;
        }

        // Lấy list các nhóm quyền về
        [HttpGet]
        public IActionResult GetRoles()
        {
            return Ok(this.entityCRUD.GetAll<Role>().ToList());
        }

        // Lấy chi tiết quyền
        [HttpGet("{id}")]
        public IActionResult GetRole(int id)
        {
            var roleFound = this.entityCRUD.GetAll<Role>(x => x.Id == id).FirstOrDefault(); 
            if(roleFound is null)
            {
                return NotFound(new ErrorModel
                {
                    Messege = "Không tìm thấy quyền",
                    Status = 404
                });
            }
            return Ok(roleFound);
        }

        // Chỉnh sửa nhóm quyền
        [HttpPut("{id}")]
        public IActionResult PutRole(int id, [FromBody] Role role)
        {
            if (id != role.Id)
            {
                return BadRequest();
            }

            role.Id = id;
            var roleFound = this.entityCRUD.GetAll<Role>(x => x.Id == id).FirstOrDefault(); 
            if(roleFound is null)
            {
                return NotFound(new ErrorModel
                {
                    Messege = "Không tìm thấy quyền này",
                    Status = 404
                });
            }

            // Phải lưu cả method vào.
            if (role.Method is null || role.Template is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Thiếu trường dữ liệu"
                });
            }

            if(this.entityCRUD.Update<Role, Role>(role, roleFound).Result)
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

        // POST: api/Roles
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public IActionResult PostRole([FromBody] Role role)
        {
            // Validate Template Url và Method ( vì Template + Method ---> Duy nhất)
            if(role.Method is null || role.Template is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Vui lòng nhập đủ các trường"
                });
            }
            var listRole = this.entityCRUD.
                GetAll<Role>(x => x.Template == role.Template && x.Method == role.Method).FirstOrDefault(); 
            if(listRole is null)
            {
                role.Id = null;
                if (this.entityCRUD.Add<Role>(role).Result)
                {
                    return Ok(role.Id);
                }
                else {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Có lỗi xảy ra vui lòng thử lại"
                    });
                }
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Quyền đã tồn tại"
                });
            }
        }

        //  Xóa nhóm quyền 
        [HttpDelete("{id}")]
        public IActionResult DeleteRole(int id)
        {
            var roleFound = this.entityCRUD.GetAll<Role>(x => x.Id == id).FirstOrDefault(); 
            if(roleFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy quyền"
                });
            }
            else
            {
                if (this.entityCRUD.Delete<Role>(roleFound).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(
                        new ErrorModel { Messege = "Có lỗi xảy ra vui lòng thử lại sau" }
                    );
                }
            }
        }

        private bool RoleExists(int id)
        {
            return context.Roles.Any(e => e.Id == id);
        }
    }
}

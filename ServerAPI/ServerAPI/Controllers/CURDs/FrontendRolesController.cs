using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrontendRolesController : ControllerBase
    {
        private  ClientManagerContext context;
        private EntityCRUDService entityCRUD;
        public FrontendRolesController(ClientManagerContext context, EntityCRUDService entityCRUD)
        {
            this.entityCRUD = entityCRUD;
            this.context = context;
        }

        // Lấy về tất cả các roleFrontend
        [HttpGet]
        public IActionResult GetFrontendRoles()
        {
            return Ok(this.entityCRUD.GetAll<FrontendRole>().Select(item=> new{ 
                item.Id, 
                item.FrontendCode, 
                item.Description
            }).ToList());
        }

        // Lấy các Router_template đã được map với mã FrontEnd
        [HttpGet]
        [Route("urls/map/{id}")]
        public IActionResult GetFrontendRole(int? id)
        {
            return Ok(this.entityCRUD.GetAll<Role>(item => item.FrontendCode == id).ToList());
        }

        // Lấy các router mà chưa được map với mã FrontEnd nào.
        [HttpGet]
        [Route("urls/unmap")]
        public IActionResult getUnmapRole()
        {
            return Ok(this.entityCRUD.GetAll<Role>(item=>item.FrontendCode == null || item.FrontendCode ==0).ToList());
        }

        // Khi mà sửa nhóm quyền frontend, thì chỉ cần sửa mô tả, không cho sửa mã code
        [HttpPut]
        [Route("{id}")]
        public IActionResult PutFrontendRole(int? id, [FromBody] FrontendRole frontendRole)
        {
            
            var frontendRoleFound = this.entityCRUD.GetAll<FrontendRole>(x => x.Id == id).FirstOrDefault();
            if (id != frontendRole.Id || frontendRoleFound.FrontendCode != frontendRole.FrontendCode)
            {
                return BadRequest();
            }
            if (frontendRoleFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy nhóm quyền"
                });
            }
            else
            {
                frontendRoleFound.Description = frontendRole.Description;
                if(this.entityCRUD.Update<FrontendRole, FrontendRole>(frontendRoleFound, frontendRole).Result)
                {
                    return Ok(id);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Có lỗi xảy ra vui lòng thử lại"
                    });
                }
            }
        }

        // Tạo mới quyền FrontEnd, và mã frontend của 1 quyền là duy nhất.
        // Và khi thêm mới thì cho nhóm quyền Admin cơ bản được đồng bộ full quyền. 
        [HttpPost]
        public IActionResult PostFrontendRole([FromBody] FrontendRole frontendRole)
        {
            var frontendRoleFound = this.entityCRUD.GetAll<FrontendRole>(x => x.FrontendCode == frontendRole.FrontendCode).
                FirstOrDefault();
            if(frontendRoleFound != null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Mã quyền FrontEnd đã tồn tại"
                });
            }
            else
            {
                frontendRole.Id = null;
                if(frontendRole.FrontendCode == 0 || frontendRole.FrontendCode < 0)
                {
                    return BadRequest(new ErrorModel { Messege = "Mã quyền front end không hợp lệ" });
                }
                if (this.entityCRUD.Add<FrontendRole>(frontendRole).Result)
                {
                    // Nếu add thành công nhóm quyền frontend mới, thì đồng bộ nhóm quyền Admin
                    var result = this.entityCRUD.Add<RoleActive>(new RoleActive
                    {
                        Id = null,
                        GroupRoleId = 2,
                        FrontendRoleId = frontendRole.Id,
                        IsCreate = true,
                        IsPut = true,
                        IsDelete = true,
                        IsView = true, 
                        isActive = true
                    }).Result;
                    if (result)
                    {
                        // Nếu thành công khi đồng bộ nhóm admin, thì động bộ tiếp những nhóm quyền còn lại ( quyền false ) 
                        var listGroupRoleNoAdmin = this.entityCRUD.GetAll<GroupRole>(x => x.Id != 2).ToList();

                        List<RoleActive> newRoleActive = new List<RoleActive>();
                        listGroupRoleNoAdmin.ForEach(item =>
                        {
                            newRoleActive.Add(new RoleActive
                            {
                                FrontendRoleId = frontendRole.Id.Value,
                                GroupRoleId = item.Id.Value,
                                IsCreate = false,
                                IsDelete = false,
                                IsPut = false,
                                IsView = false,
                                isActive = false
                            }); ;
                        });
                        if (this.entityCRUD.AddRange<RoleActive>(newRoleActive).Result)
                        {
                            return Ok();
                        }
                        else
                        {
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
                            Messege = "Có lỗi xảy ra vui lòng thử lại"
                        });
                    }

                }
                else
                {
                    return BadRequest(new ErrorModel { Messege = "Có lỗi xảy ra vui lòng thử lại" });
                }
            }
        }

        // Khi xóa một nhóm quyền nào đó trên frontent, thì cần 
        // + Update nhóm Api tại trường frontendCode trong bảng role, 
        // + Xóa nhóm quyền đó với nhóm User Trong bảng roleActive, 
        //+ Xóa trong bảng FrontEndRole, Id ở đây là mã quyền, k phải là trường Id trong bảng.
        [HttpDelete("{id}")]
        public IActionResult DeleteFrontendRole(int? id)
        {
            var frontEndRoleFound = this.entityCRUD.GetAll<FrontendRole>(x => x.FrontendCode == id).FirstOrDefault();
            if(frontEndRoleFound is null)
            {
                return BadRequest(new ErrorModel { Messege = "Không tìm thấy nhóm quyền" });
            }
            // Update nhóm API
            var listApi = this.entityCRUD.GetAll<Role>(x => x.FrontendCode == id).ToList();
            listApi.ForEach(item => item.FrontendCode = 0);
            var updateApiResult = this.entityCRUD.UpdateRange<Role>(listApi).Result;

            if(frontEndRoleFound != null)
            {
                // Xóa nhóm quyền đó trong bảng roleAactive. 
                var listRoleActiveFound = this.entityCRUD.GetAll<RoleActive>(x => x.FrontendRoleId == frontEndRoleFound.Id).ToList();
                var removeRoleActiveResult = this.entityCRUD.DeleteRange<RoleActive>(listRoleActiveFound).Result;
                // Xóa quyền đó trong bảng frontEnd
                var removeFrontendRoleResult =  this.entityCRUD.Delete<FrontendRole>(frontEndRoleFound).Result;
                return Ok(id);
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Nhóm quyền frontend không tồn tại"
                });
            }
        }

    }
}

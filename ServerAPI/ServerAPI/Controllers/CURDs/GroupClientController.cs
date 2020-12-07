using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;
using ServerAPI.Model.StaticModel;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupClientController : ControllerBase
    {
        private ClientManagerContext context;
        private EntityCRUDService entityCRUD; 

        public GroupClientController(ClientManagerContext context, EntityCRUDService entityCRUD)
        {
            this.context = context;
            this.entityCRUD = entityCRUD;
        }

        // Lấy tất cả các nhóm về. 
        [HttpGet]
        public IActionResult getAllGroup()
        {
            return Ok(this.entityCRUD.GetAll<GroupClient>().Select(m => new
            {
                m.Id,
                m.Price,
                m.Desciption
            }).ToList());
        }

        // Lấy chi tiết nhóm máy
        [HttpGet("{id}")]
        public IActionResult getGroupClientDetail(int id)
        {
           var group = this.context.GroupClients.FirstOrDefault(group => group.Id == id);
           if(group is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy nhóm máy",
                    Status = 400
                });
            }
            else
            {
                return Ok(group);
            }
        }

        // Thêm mới nhóm máy
        [HttpPost]
        public IActionResult addGroupClient([FromBody] GroupClient group)
        {
            group.Id = null;
            if (this.entityCRUD.Add<GroupClient>(group).Result)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Vui lòng nhập đầy đủ các trường",
                    Status = 400
                });
            }
        }

        // Sửa nhóm máy
        [HttpPut("{id}")]
        public IActionResult updateGroupClient(int id, [FromBody] GroupClient group)
        {
            group.Id = id;
            var groupFound =  this.context.GroupClients.FirstOrDefault(x => x.Id == id); 
            if(groupFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy nhóm máy",
                    Status = 400
                });
            }
            else
            {
                if (this.entityCRUD.Update<GroupClient, GroupClient>(group, groupFound).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Vui lòng nhập đầy đủ các trường",
                        Status = 400
                    });
                }
            }
        }

        //Xóa nhóm máy
        [HttpDelete("{id}")]
        public IActionResult deleteGroup(int id)
        {
            if (StaticConsts.UndeletableGroupRole.Contains(id))
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Nhóm máy mặc định không thể xóa"
                });
            }
           var groupFound = this.context.GroupClients.FirstOrDefault(group => group.Id == id);
           if(groupFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy nhóm máy",
                    Status = 400
                }); 
            }
            else
            {
                if(this.context.Clients.Where(item => item.ClientGroupId == groupFound.Id).ToArray().Length > 0)
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Không thể xóa nhóm máy này vì đang còn máy ở trong"
                    });
                }

                if (this.entityCRUD.Delete<GroupClient>(groupFound).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Vui lòng nhập đầy đủ các trường cần thiết",
                        Status = 400
                    });
                }
            }
        }
    }
}

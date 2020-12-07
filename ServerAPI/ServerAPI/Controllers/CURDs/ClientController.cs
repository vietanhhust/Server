using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private ClientManagerContext context;
        private EntityCRUDService entityCRUD;
        public ClientController(ClientManagerContext context, EntityCRUDService entityCRUD)
        {
            this.context = context;
            this.entityCRUD = entityCRUD;
        }



        // Lấy danh sách tất cả các client về
        [HttpGet]
        public IActionResult getAllClient()
        {
            return Ok(this.entityCRUD.GetAll<Client>().ToList());
        }

        // Lấy về chi tiêt client
        [HttpGet("{id}")]
        public IActionResult getClientDetail(int id)
        {
            var clientFound = this.context.Clients.FirstOrDefault(item => item.Id == id);
            if (clientFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy client",
                    Status = 400
                });
            }
            else
            {
                return Ok(clientFound);
            }
        }

        // Lấy về list cliet theo nhóm máy
        [HttpGet]
        [Route("/group/{id}/clients")]
        public IActionResult getClientOfGroup(int? id)
        {
            var group = this.context.GroupClients.FirstOrDefault(x => x.Id == id); 
            if(group is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy group chỉ định",
                    Status = 400
                });
            }
            else
            {
                return Ok(this.entityCRUD.GetAll<Client>().Where(client => client.ClientGroupId == group.Id));
            }
        }

        // Thêm mới client
        [HttpPost]
        public IActionResult addClient([FromBody] Client client)
        {
            client.Id = null;
            client.ClientGroupId = 1;
            if (this.entityCRUD.Add<Client>(client).Result)
            {
                return Ok(true); 
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Thao tác thêm thất bại, vui lòng điền đầy đủ các trường",
                    Status = 400
                }); 
            }
        }

        // Sửa thông tin client, nhưng ở đây không cho phép sửa trường CLientId; 
        [HttpPut("{id}")]
        public IActionResult updateClient(int id, [FromBody] Client client)
        {
            client.Id = id;
            client.IsNew = false;
            var clientFound = this.context.Clients.FirstOrDefault(client => client.Id == id); 
            if(clientFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy Client", 
                    Status = 400
                });
            }
            else
            {
                client.ClientId = clientFound.ClientId;
                if(this.entityCRUD.Update<Client, Client>(client, clientFound).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Status = 400, 
                        Messege = "Thao tác sửa thất bại"
                    }); 
                }
            }

        }


        // Thêm mới Client, nhưng Client Id mới sẽ là tự đánh, bằng cách tìm số máy lớn nhất rồi cộng thêm 1;
        [HttpPost]
        public IActionResult Post([FromBody] Client client)
        {
            
            var maxId = this.GetMaxClientId();
            client.ClientId = maxId == 0 ? 1 : maxId + 1;
            client.IsNew = true; 
            if (this.entityCRUD.Add<Client>(client).Result)
            {
                return Ok(client.ClientId);
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Vui lòng điền đủ các trường"
                }); 
            }
        }

        // Hàm phụ để tìm số máy lớn nhất 
        private int GetMaxClientId()
        {
            var maxId = 0;
            var clients =  this.entityCRUD.GetAll<Client>().ToList();
            if(clients.Count > 0)
            {
                maxId = clients.Max(item => item.ClientId);
            }
            return maxId;
        }

        // Khi phần mềm mới cài đặt,


        // Đánh lại tất cả số thứ tự. Yêu cầu khi đánh là phải đánh hết, đánh không trùng
        [HttpPost]
        [Route("reIdentity")]
        public IActionResult creatLabelForClient()
        {
            return null; 
        }

        // Check xem Id máy có tồn tại không. 

















        // PUT api/<ClientController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ClientController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

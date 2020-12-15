using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServerAPI.Controllers.Services;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagingAccountsController : ControllerBase
    {
        private ClientManagerContext context;
        private EntityCRUDService entityCRUD;
        private PasswordService passwordService;
        public ManagingAccountsController(ClientManagerContext context, EntityCRUDService entityCRUD,
            PasswordService passwordService
        )
        {
            this.context = context;
            this.entityCRUD = entityCRUD;
            this.passwordService = passwordService;
        }

        // GET: api/ManagingAccounts
        // Lấy về các tài khoản quản trị
        [HttpGet]
        public IActionResult GetManagingAccounts()
        {
            return Ok(this.entityCRUD.GetAll<ManagingAccount>().ToList()); 
        }

        // GET: api/ManagingAccounts/5
        // Lấy về chi tiết tài khoản
        [HttpGet("{id}")]
        public IActionResult GetManagingAccount(int id)
        {
            var managingAccount = this.entityCRUD.GetAll<ManagingAccount>(x => x.Id == id).FirstOrDefault(); 


            if (managingAccount == null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản này"
                });
            }

            return Ok(managingAccount);
        }

        // Sửa thông tin tài khoản
        [HttpPut("{id}")]
        public  IActionResult PutManagingAccount(int id, [FromBody] ManagingAccount managingAccount)
        {
            managingAccount.Id = id;
            if (id == 1)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tài khoản không thể sửa"
                });
            }
            var managingAccountFound = this.entityCRUD.GetAll<ManagingAccount>(x => x.Id == id).FirstOrDefault(); 
            if(managingAccountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản"
                });
            }
            else
            {
                managingAccount.Password = managingAccountFound.Password;
                if(this.entityCRUD.Update<ManagingAccount, ManagingAccount>(managingAccount, managingAccountFound).Result)
                {
                    return Ok(true);
                }
                else{
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Vui lòng nhập đầy đủ các trường cần thiết"
                    });
                }
            }
        }

        // Đổi mật khẩu cho tài khoản 
        [HttpPut]
        [Route("changepass/{id}")]
        public IActionResult changePassword(int id, [FromBody] PasswordModel password)
        {
            var accountFound = this.entityCRUD.GetAll<ManagingAccount>(x => x.Id == id).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản"
                });
            }
            else
            {
                var newPassword = this.passwordService.PasswordHash(password.Password);
                accountFound.Password = newPassword; 
                if(this.entityCRUD.Update<ManagingAccount, ManagingAccount>(accountFound, accountFound).Result)
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
        }



        // Tạo mới tài khoản
        [HttpPost]
        public IActionResult PostManagingAccount([FromBody] ManagingAccount managingAccount)
        {
            if (managingAccount.Password is null || managingAccount.Name is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Vui lòng điền tên và mật khẩu"
                });
            }
            // Check tên tài khoản trùng
            if (this.entityCRUD.GetAll<ManagingAccount>().Any(x => x.Name.ToLower()==managingAccount.Name.ToLower())) {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tên tài khoản đã tồn tại"
                });
            }
            else
            {
               
                var passwordHash = this.passwordService.PasswordHash(managingAccount.Password);
                managingAccount.Password = passwordHash;
                if(managingAccount.GroupRoleId is null)
                {
                    managingAccount.GroupRoleId = 1;
                }
                if (this.entityCRUD.Add<ManagingAccount>(managingAccount).Result)
                {
                    return Ok(managingAccount.Id);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Vui lòng nhập các trường đầy đủ"
                    });
                }

            }
        }

        // Xóa tài khoản
        [HttpDelete("{id}")]
        public IActionResult DeleteManagingAccount(int id)
        {
            if (id == 1)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tài khoản quản trị không thể xóa"
                });
            }
            var accountFound = this.entityCRUD.GetAll<ManagingAccount>(x => x.Id == id).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản"
                });
            }
            else
            {
                if (this.entityCRUD.Delete<ManagingAccount>(accountFound).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Có lỗi xảy ra vui lòng thử lại sau"
                    });
                }
            }
        }

        // Api nạp tiền: Xử lý tiền tệ riêng.
        // Có bảng ghi lại lịch sử nạp



        private bool ManagingAccountExists(int id)
        {
            return context.ManagingAccounts.Any(e => e.Id == id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Controllers.Services;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;
using ServerAPI.Model.Hubs;
using ServerAPI.Model.StaticModel;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ClientManagerContext context;
        private EntityCRUDService entityCRUD;
        private PasswordService passwordService;
        private IHubContext<ClientHub> hubContext;
        public AccountsController(ClientManagerContext context, EntityCRUDService entityCRUD,
            PasswordService passwordService, IHubContext<ClientHub> hubContext)
        {
            this.entityCRUD = entityCRUD;
            this.passwordService = passwordService;
            this.context = context;
            this.hubContext = hubContext; 
        }

        // Lấy ra account
        // GET: api/Accounts
        [HttpGet]
        public IActionResult GetAccounts()
        {
            return Ok(this.entityCRUD.GetAll<Account>().ToList());
        }

        // Lấy chi tiết 1 accout
        [HttpGet("{id}")]
        public IActionResult GetAccount(int? id)
        {
            var accountFound = this.entityCRUD.GetAll<Account>(x => x.Id == id).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản"
                });
            }
            else
            {
                return Ok(accountFound);
            }
        }

        // Search Account
        [HttpGet]
        [Route("search")]
        public IActionResult searchAccount([FromQuery] string keyword)
        {
            if(keyword is null)
            {
                return Ok(this.entityCRUD.GetAll<Account>().ToList());
            }
            else
            {
                return Ok(this.entityCRUD.GetAll<Account>(x =>
                    x.AccountName.ToLower().Contains(keyword.ToLower())));
            }
        }


        // Sửa tài khoản
        // Không cho sửa mật khẩu.
        // Không cho đổi tên.
        // Không cho đổi trạng thái đăng nhập.
        [HttpPut("{id}")]
        public IActionResult PutAccount(int? id,[FromBody] Account account)
        {
            // Check tài khoản có tồn tại không
            var accountFound = this.entityCRUD.GetAll<Account>(x => x.Id == id).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tài khoản không tồn tại"
                });
            }
            else
            {
                account.Id = id;
                account.AccountName = accountFound.AccountName;
                account.Password = accountFound.Password;
                account.IsLogged = accountFound.IsLogged;
                if (this.entityCRUD.Update<Account, Account>(account, accountFound).Result)
                {
                    return Ok(true);
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
        
        // Đổi mật khẩu tài khoản. 
        [HttpPut]
        [Route("changepass/{id}")]
        public IActionResult changePassword(int id, PasswordModel password)
        {
            var accountFound = this.entityCRUD.GetAll<Account>(x => x.Id == id).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản"
                });
            }
            else
            {
                accountFound.Password = this.passwordService.PasswordHash(password.Password);
                if (this.entityCRUD.Update<Account, Account>(accountFound, accountFound).Result) {
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

        // Tạo mới tài khoản
        [HttpPost]
        public IActionResult PostAccount([FromBody] Account account)
        {
            if (account.Password is null || account.AccountName is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Vui lòng nhập đầy đủ mật khẩu và tên"
                });
            }
            // Kiểm tra tài khoản có trùng tên không
            var existAccount = this.entityCRUD.GetAll<Account>(x => x.AccountName.ToLower() == account.AccountName.ToLower()).ToList();
            if(existAccount.Count > 0)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tài khoản đã tồn tại"
                });
            }
           
            account.AccountName = account.AccountName.ToLower(); 
            account.IsLogged = false;
            account.IsActived = true;
            account.Password = this.passwordService.PasswordHash(account.Password);
            if (this.entityCRUD.Add<Account>(account).Result)
            {
                return Ok(account.Id);
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Vui lòng nhập đầy đủ tên và mật khẩu"
                });
            }
        }

        // Xóa tài khoản: Bằng cách cho set Active là nó không hoạt động
        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(int? id)
        {
            var accountFound = this.entityCRUD.GetAll<Account>(x => x.Id == id).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản"
                });
            }
            else
            {
                accountFound.IsActived = false;
                if(this.entityCRUD.Update<Account, Account>(accountFound, accountFound).Result)
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

        // Test APi nạp tiền
        [HttpPut]
        [Route("balance")]
        public IActionResult addBalance([FromBody] BalanceModel model)
        {
            var acc= this.entityCRUD.GetAll<Account>(x => x.Id==model.AccountId).FirstOrDefault();
            if(acc is null)
            {
                return BadRequest(new ErrorModel() {Messege = "Tài khoản không đúng" });
            }
            acc.Balance += model.Money;
            if(this.entityCRUD.Update<Account, Account>(acc, acc).Result)
            {
                return Ok();
            }
            else
            {
                return BadRequest(); 
            }
            // Hub context bắn về 
            

        }

        private bool AccountExists(int? id)
        {
            return context.Accounts.Any(e => e.Id == id);
        }
    }
}

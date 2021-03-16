using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        private IHubContext<AdminPageHub> adminHub; 
        public AccountsController(ClientManagerContext context, EntityCRUDService entityCRUD,
            PasswordService passwordService, IHubContext<ClientHub> hubContext, IHubContext<AdminPageHub> adminHub)
        {
            this.entityCRUD = entityCRUD;
            this.passwordService = passwordService;
            this.context = context;
            this.hubContext = hubContext;
            this.adminHub = adminHub; 
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
                return Ok(this.entityCRUD.GetAll<Account>(x=>x.IsActived == true).ToList());
            }
            else
            {
                return Ok(this.entityCRUD.GetAll<Account>(x =>
                    x.AccountName.ToLower().Contains(keyword.ToLower()) && x.IsActived == true));
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

            account.Debit = 0;
            account.ElaspedTime = 0; 
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
                if (accountFound.IsLogged.Value)
                {
                    return BadRequest(new ErrorModel()
                    {
                        Messege = "Không được xóa tài khoản đang hoạt động"
                    });
                }
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
            // Tìm tài khoản của admin gửi Đến. 
            var adminName = this.HttpContext.User.Claims.First(c => c.Type == "Name").Value;
            Console.WriteLine(adminName);
            var adminFound = this.entityCRUD.GetAll<ManagingAccount>(x => x.Name == adminName).FirstOrDefault();

            if (model.Money < 0)
            {
                return BadRequest(new ErrorModel()
                {
                    Messege = "Số tiền không hợp lệ"
                });
            }
            var acc= this.entityCRUD.GetAll<Account>(x => x.Id==model.AccountId).FirstOrDefault();
            if(acc is null)
            {
                return BadRequest(new ErrorModel() {Messege = "Tài khoản không đúng" });
            }
            acc.Balance += model.Money;
            acc.ElaspedTime += model.Money;
            if(this.entityCRUD.Update<Account, Account>(acc, acc).Result)
            {
                // Gửi số tiền mới về tài khoản đang kết nối
                // Đoạn này đang hardcode, 
                //nếu mà trong đám client connect tài khoản đang đăng nhập thì lấy ra gửi tiền mới
                ClientConnect clientConnected = null;
                StaticConsts.ConnectedClient.ForEach(item =>
                {
                    if (item.Account is null)
                    {

                    }
                    else
                    {
                        if (item.Account.Id == model.AccountId) {
                            clientConnected = item;
                        }
                    }
                });

                if(clientConnected is null)
                {

                }
                else
                {
                    this.hubContext.Clients.
                        Client(clientConnected.ConnectionId).
                            SendAsync("balanceChange", (float)acc.Balance);
                    
                }

                if (adminFound != null)
                {
                    // typeChange = true tức là trừ tiền. 
                    var result = this.entityCRUD.Add<HistoryChangeBalance>(new HistoryChangeBalance()
                    {
                        AccountId = acc.Id.Value, 
                        Cost = model.Money, 
                        ManagingAccountId = adminFound.Id.Value,
                        TypeChange = false, 
                        TimeChange = DateTimeOffset.Now.ToUnixTimeSeconds(), 
                    }).Result;


                    if (result)
                    {
                        if(clientConnected != null)
                        {
                            clientConnected.Account.Balance += model.Money;
                        }
                        this.adminHub.Clients.All.SendAsync("dashboard", StaticConsts.ConnectedClient);
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(); 
                    }
                }
                else
                {
                    return Ok(); 
                }
            }
            else
            {
                return BadRequest(); 
            }
        }


        [HttpPut]
        [Route("refund")]
        public IActionResult refundBalance([FromBody] BalanceModel model)
        {
            // Tài khoản admin
            var adminName = this.HttpContext.User.Claims.First(c => c.Type == "Name").Value;
            //Console.WriteLine(adminName);
            var adminFound = this.entityCRUD.GetAll<ManagingAccount>(x => x.Name == adminName).FirstOrDefault();


            if (model.Money < 0)
            {
                return BadRequest(new ErrorModel()
                {
                    Messege = "Số tiền không hợp lệ"
                });
            }
            var accountFound = this.entityCRUD.GetAll<Account>(x => x.Id == model.AccountId).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel()
                {
                    Messege = "Tài khoản không tồn tại"
                });
            }
            if(accountFound.Balance < model.Money)
            {
                return BadRequest(new ErrorModel()
                {
                    Messege = "Số tiền không được lớn hơn số dư của tài khoản"
                });
            }
            accountFound.Balance -= model.Money;
            accountFound.ElaspedTime -= model.Money;
            if(this.entityCRUD.Update<Account, Account>(accountFound, accountFound).Result)
            {
                ClientConnect clientConnected = null;
                StaticConsts.ConnectedClient.ForEach(item =>
                {
                    if (item.Account is null)
                    {

                    }
                    else
                    {
                        if (item.Account.Id == model.AccountId)
                        {
                            clientConnected = item;
                        }
                    }
                });

                if (clientConnected is null)
                {

                }
                else
                {
                    this.hubContext.Clients.
                        Client(clientConnected.ConnectionId).
                            SendAsync("balanceChange", (float)accountFound.Balance);
                    
                }

                var result = this.entityCRUD.Add<HistoryChangeBalance>(new HistoryChangeBalance()
                {
                    AccountId = accountFound.Id.Value,
                    ManagingAccountId = adminFound.Id.Value,
                    Cost = model.Money,
                    TimeChange = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    TypeChange = true
                }).Result;

                if (result)
                {
                    if(clientConnected != null)
                    {
                        clientConnected.Account.Balance -= model.Money;
                    }
                    this.adminHub.Clients.All.SendAsync("dashboard", StaticConsts.ConnectedClient);
                    return Ok();
                }
                else
                    return BadRequest();
            }
            else
            {
                return BadRequest(new ErrorModel()
                {
                    Messege = "Có lỗi xảy ra vui lòng thử lại sau"
                });
            }
        }

        [Route("testAdd")]
        public IActionResult testAdd()
        {
            //var result = this.entityCRUD.Add<HistoryChangeBalance>().Result;
            this.context.HistoryChangeBalances.Add(new HistoryChangeBalance()
            {
                AccountId = 1,
                Cost = 5000,
                ManagingAccountId = 1,
                TypeChange = true,
                TimeChange = DateTimeOffset.Now.ToUnixTimeSeconds(),
            });
            this.context.SaveChanges(); 
            return Ok();

        }
    }
}

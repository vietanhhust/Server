using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Controllers.Services;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;
using ServerAPI.Model.StaticModel;

namespace ServerAPI.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private IConfiguration Configuration;
        private ClientManagerContext context;
        private EntityCRUDService entityCRUD;
        private PasswordService passwordService;
        public AuthenticationController(IConfiguration configuration, ClientManagerContext context, 
            EntityCRUDService entityCRUD, PasswordService passwordService
        )
        {
            this.Configuration = configuration;
            this.context = context;
            this.entityCRUD = entityCRUD;
            this.passwordService = passwordService; 
        }

        // Authentication cho tài khoản quản lý. 
        [HttpPost]
        [Route("managing/token")]
        public IActionResult LoginForAdmin([FromBody] LoginModel model)
        {
            if(model.Password is null || model.UserName is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Thiếu tên đăng nhập hoặc mật khẩu"
                });
            }
            var passwordHash = this.passwordService.PasswordHash(model.Password);
            var accountFound =  this.entityCRUD.GetAll<ManagingAccount>(x => x.Name == model.UserName && x.Password == passwordHash).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản"
                });
            }
            else
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("Name", accountFound.Name),
                        new Claim("GroupRole", accountFound.GroupRoleId.ToString()),
                        new Claim("Description", accountFound.Description)
                    }),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration.GetSection("SecretKey").Value)),
                        SecurityAlgorithms.HmacSha256Signature),
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                var activeRoles = this.entityCRUD.
                    GetAll<RoleActive>(x => x.GroupRoleId == accountFound.GroupRoleId).
                    ToList().Select(x=> {
                        var frontendRoleFound = this.entityCRUD.GetAll<FrontendRole>(role => role.Id == x.FrontendRoleId).FirstOrDefault();
                        return new
                        {
                            x.Id,
                            //x.IsView, x.IsCreate, x.IsPut, x.IsDelete, 
                            x.IsActive,
                            x.GroupRoleId,
                            frontendCode = frontendRoleFound.FrontendCode, 
                            description = frontendRoleFound.Description, 
                            frontendRoleId = frontendRoleFound.Id
                        };
                    });
                return Ok(new
                {
                    token = token,
                    name = accountFound.Name,
                    groupRoleId = accountFound.GroupRoleId,
                    role = activeRoles
                });
            }
        }


        //Authentication cho tài khoản người chơi. 
        // Khi đăng nhập thành công thì cho người chơi vào list ClientCOnnected.
        // Id ở đây là clientId
        [HttpPost]
        [Route("account/{clientId}")]
        public IActionResult customerAuthentication(int clientId, [FromBody] LoginModel model)
        {

            var clientFound = this.entityCRUD.GetAll<Client>(x => x.ClientId == clientId).FirstOrDefault();
            var groupClientFound = this.entityCRUD.GetAll<GroupClient>(x => x.Id == clientFound.ClientGroupId).FirstOrDefault(); 
            if(model.UserName is null || model.Password is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tài khoản không đúng"
                });
            }

            var passwordHash = this.passwordService.PasswordHash(model.Password);
            var accoundFound = this.entityCRUD.GetAll<Account>(x => x.AccountName.ToLower() == model.UserName.ToLower() &&
                x.Password == passwordHash).FirstOrDefault();

            if (accoundFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tên tài khoản hoặc mật khẩu không đúng"
                });
            }
            else
            {
                // Check so tien con lai trong tai khoan
                var time = (float)accoundFound.Balance / (float)groupClientFound.Price * (float)60;
                if (time <= 2)
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Thời gian sử dụng đã hết, vui lòng nạp thêm"
                    });
                }

                // Không cho tài khoản đăng nhập ở chỗ khác
                if (accoundFound.IsLogged.HasValue ? accoundFound.IsLogged.Value: false)
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Tài khoản đang được sử dụng"
                    });
                }

                accoundFound.IsLogged = true; 
                if(this.entityCRUD.Update<Account, Account>(accoundFound, accoundFound).Result)
                {
                    StaticConsts.ConnectedClient.Where(item => item.ClientId == clientId).FirstOrDefault().Account = accoundFound;
                    Console.WriteLine(JsonConvert.SerializeObject(StaticConsts.ConnectedClient));
                    return Ok(accoundFound);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Máy chủ lỗi"
                    });
                }
            }
        }

        [HttpPost]
        [Route("account/logout/{clientId}")]
        public IActionResult logoutAccount(int clientId, [FromBody] LoginModel model)
        {
            var accountFound = this.entityCRUD.GetAll<Account>(x => x.AccountName.ToLower() == model.UserName.ToLower()).FirstOrDefault(); 
            if(accountFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy tài khoản"
                });
            }
            else
            {
                // Validate để k bị lợi dụng api tắt máy người khác
                if(StaticConsts.ConnectedClient.
                    Where(item=>item.ClientId==clientId).
                        FirstOrDefault().Account?.AccountName != accountFound.AccountName)
                {
                    return BadRequest(new ErrorModel() { Messege = "Logout tài khoản không đúng" });
                }

                accountFound.IsLogged = false; 
                if(this.entityCRUD.Update<Account, Account>(accountFound, accountFound).Result)
                {
                    StaticConsts.ConnectedClient.Where(item => item.ClientId == clientId).FirstOrDefault().Account = null; 
                    return Ok(StaticConsts.ConnectedClient);
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

        [HttpGet]
        [Route("ping")]
        public IActionResult Test()
        {
           return Ok(JsonConvert.SerializeObject(StaticConsts.ConnectedClient));
        }
    }
}

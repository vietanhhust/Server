using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class OrderController : ControllerBase
    {
        private EntityCRUDService entityCRUD;
        private PasswordService passwordService;
        private IHubContext<ClientHub> hubContext;
        public OrderController(ClientManagerContext context, EntityCRUDService entityCRUD,
            PasswordService passwordService, IHubContext<ClientHub> hubContext)
        {
            this.entityCRUD = entityCRUD;
            this.passwordService = passwordService;
            this.hubContext = hubContext;
        }

        [HttpPost]
        [Route("neworder")]
        public IActionResult CreateNewOrder([FromBody] OrderCreating order)
        {

            Order newOrder = new Order()
            {
                AccountId = order.AccountId,
                AdminId = order.AdminId,
                CreatedTime = order.CreatedTime,
                ClientId = order.ClientId,
                Status = true,
            };
            
            var result = this.entityCRUD.Add<Order>(newOrder);
            if (result.Result)
            {
                var lstCategory = order.ListCategory;
                var listOrderDetail = new List<OrderDetail>();
                lstCategory.ForEach(item =>
                {
                    listOrderDetail.Add(new OrderDetail()
                    {
                        Amount = item.Quantity, 
                        CategoryItemId = item.CategoryId, 
                        OrderId = newOrder.Id.Value, 
                    });
                });

               var addDetailResult = this.entityCRUD.AddRange<OrderDetail>(listOrderDetail).Result;
                if (addDetailResult)
                {
                    return Ok(newOrder.Id);
                }
                else
                {
                    return BadRequest(new ErrorModel() { Messege = "Lỗi add range order" });
                }
            }
            else
            {
                return BadRequest(); 
            }
        }

        //[HttpPost]
        //[Route("testOrder")]
        //public async IActionResult testOrder()
        //{
        //   await this.entityCRUD.Add<Order>(new Order()
        //    {
        //       AccountId = null, 
        //       Admin = null, 
        //       ClientId = null
        //    });
        //    return Ok();
        //}
    }
}

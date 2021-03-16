using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ServerAPI.Controllers.Services;
using ServerAPI.Model.Database;
using ServerAPI.Model.Hubs;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly ClientManagerContext context;
        private EntityCRUDService entityCRUD;
        private PasswordService passwordService;
        private IHubContext<ClientHub> hubContext;
        private IHubContext<AdminPageHub> adminHub;
        public HistoryController(ClientManagerContext context, EntityCRUDService entityCRUD,
            PasswordService passwordService, IHubContext<ClientHub> hubContext, IHubContext<AdminPageHub> adminHub)
        {
            this.entityCRUD = entityCRUD;
            this.passwordService = passwordService;
            this.context = context;
            this.hubContext = hubContext;
            this.adminHub = adminHub;
        }


        [HttpPost]
        [Route("balance")]
        public IActionResult getHistoryBalanceChange([FromBody] HistoryBalanceQueryModel queryModel)
        {
            // Test

            var accs = this.context.Accounts.ToList();
            var ads = this.context.ManagingAccounts.ToList();
            var orders = this.context.Orders.ToList();
            var orderDetails = this.context.OrderDetails.ToList();

            
            if (queryModel.accountId ==0 || queryModel.accountId is null)
            {
                return Ok(this.entityCRUD.GetAll<HistoryChangeBalance>(item => item.TimeChange <= queryModel.toDate &&
                    item.TimeChange > queryModel.fromDate && item.TypeChange == queryModel.typeChange
                ));
            }
            else
            {
                return Ok(this.entityCRUD.GetAll<HistoryChangeBalance>(item => item.TimeChange <= queryModel.toDate &&
                    item.TimeChange > queryModel.fromDate && item.TypeChange == queryModel.typeChange && item.AccountId == queryModel.accountId
                ).ToList());
            }
            
        }


        [HttpPost]
        [Route("order")]
        public IActionResult getOrderHistory([FromBody] HistoryOrderQueryModel queryModel) 
        {
            var listOrder = this.entityCRUD.GetAll<Order>(x => x.CreatedTime < queryModel.toDate && x.CreatedTime > queryModel.fromDate); 
            
            if(queryModel.adminId != null && queryModel.adminId !=0)
            {
                listOrder = listOrder.Where(item => item.AdminId == queryModel.adminId).ToList();
            }
            if(queryModel.accountId != null && queryModel.accountId !=0)
            {
                listOrder = listOrder.Where(item => item.AccountId == queryModel.accountId).ToList();
            }
            if(queryModel.clientId != null && queryModel.clientId != 0)
            {
                listOrder = listOrder.Where(item => item.ClientId == queryModel.clientId).ToList(); 
            }


            return Ok(listOrder);
        }

        [HttpGet]
        [Route("details/{id}")]
        public IActionResult getOrderDetail(int id)
        {
            return Ok(this.entityCRUD.GetAll<OrderDetail>(x => x.OrderId == id)); 
        }

    }



}

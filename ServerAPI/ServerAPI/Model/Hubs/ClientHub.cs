using Microsoft.AspNetCore.SignalR;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.Model.StaticModel;
using Newtonsoft.Json;

namespace ServerAPI.Model.Hubs
{
    public class ClientHub: Hub
    {
        private EntityCRUDService entityCRUD;

        // Tài khoản nào, sử dụng máy nào. 
        private int CLientId = 0;
        private int UserId = 0;


        // Lấy hubs để giao tiếp với các pageAdmin
        IHubContext<AdminPageHub> adminHubs;
        public ClientHub(EntityCRUDService entityCRUD, IHubContext<AdminPageHub> adminHubs)
        {
            this.adminHubs = adminHubs;
            this.entityCRUD = entityCRUD;
        }

        public override Task OnConnectedAsync()
        {
            
            // Lấy id tài khoản và id máy đang sử dụng  (id máy là số máy, k phải identity)
            var CLientId = Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["Client-Id"]);
            var UserId = Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["User-Id"]);
            Console.WriteLine("User " + UserId + "Client " + CLientId);

            // Đổi trạng thái của ListClient connected
            var connected = StaticConsts.ConnectedClient.Where(x => x.ClientId == CLientId).FirstOrDefault();
            connected.ConnectionId = this.Context.ConnectionId;

             
            connected.Account = this.entityCRUD.GetAll<Account>(x => x.Id == UserId).FirstOrDefault();
           
            var client = this.entityCRUD.GetAll<Client>(x => x.ClientId == CLientId).FirstOrDefault();
            StaticConsts.ConnectedClient.ForEach(item =>
            {
                Console.WriteLine("Clien-Id: " + item.ClientId + ", ConnectedId: " + item.ConnectionId);
            });
            Console.WriteLine("Máy: " + CLientId);
            return base.OnConnectedAsync();
        }

        // Chỉnh lại trạng thái của static list ClientConnected
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var CLientId = Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["Client-Id"]);
            var UserId = Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["User-Id"]);
            var connectedFound = StaticConsts.ConnectedClient.Where(x => x.ClientId== CLientId).FirstOrDefault();
            connectedFound.ConnectionId = "";

            if(connectedFound.Account is null)
            {

            }
            else
            {
                var accountFound = this.entityCRUD.GetAll<Account>(x => x.Id == connectedFound.Account.Id.Value).FirstOrDefault();
                accountFound.IsLogged = false; 
                var result = this.entityCRUD.Update<Account, Account>(accountFound, accountFound).Result;
                if(this.entityCRUD is null)
                {
                    Console.WriteLine("khong co service");
                }
                connectedFound.Account = null;
            }
            
            return base.OnDisconnectedAsync(exception);
        }


        // Hàm này sẽ được gọi 3 phút 1 lần. 
        // Update tài khoản, trừ tiền.
        [HubMethodName("bill")]
        public void cost()
        {
            // Đoạn này cần xem xét về kiểu int và float
            // Lấy nhóm máy ---> giá. 
            var client = this.entityCRUD.GetAll<Client>(x => 
                x.ClientId == Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["Client-Id"])).
                FirstOrDefault();

            var UserId = Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["User-Id"]);
            var clientGroup = this.entityCRUD.GetAll<GroupClient>(x => x.Id == client.ClientGroupId).
                FirstOrDefault();
            var cost = clientGroup.Price / 60f;

            var account = this.entityCRUD.GetAll<Account>(x => x.Id == UserId).FirstOrDefault();
            account.Balance -= (int)Math.Round(cost);
            Console.WriteLine("trừ tiền: {0}", (int)Math.Round(cost));
            var updateResult = this.entityCRUD.Update<Account, Account>(account, account).Result;
            Console.WriteLine(cost);
            // Khi trừ tiền, thì phát ra cho trang admin biết. Dùng IHubContext, đoạn này viết sau.
        }

        // Gửi cho server, ở bên hub khác. 
        [HubMethodName("sendToAdmin")]
        public void chatWithAdmin(string mess, string accountName)
        {
            Console.WriteLine(accountName + " : " + mess);
            var clientId = Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["Client-Id"]);
            this.adminHubs.Clients.All.SendAsync("fromClient", mess, accountName, this.Context.ConnectionId, clientId);
        }

        // Nhận tin nhắn từ hub bên server
        [HubMethodName("recieveFromAdmin")]
        public Task receiveMess(string mess, string connectionId)
        {
            return this.Clients.Clients(this.Context.ConnectionId).SendAsync("serverReply", mess);
        }

        
    }
}

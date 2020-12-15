using Microsoft.AspNetCore.SignalR;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.Model.StaticModel;

namespace ServerAPI.Model.Hubs
{
    public class ClientHub: Hub
    {
        private EntityCRUDService entityCRUD;

        // Tài khoản nào, sử dụng máy nào. 
        private int CLientId = 0;
        private int UserId = 0;

        // Các biến tài khoản, CLient được tìm thấy 
        private Client client;
        private ClientConnect connected;
        private Account account;

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
            this.CLientId = Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["Client-Id"]);
            this.UserId = Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["User-Id"]);

            // Đổi trạng thái của ListClient connected
            this.connected = StaticConsts.ConnectedClient.Where(x => x.ClientId == this.CLientId).FirstOrDefault();
            connected.ConnectionId = this.Context.ConnectionId;

            this.account = this.entityCRUD.GetAll<Account>(x => x.Id == this.CLientId).FirstOrDefault();
            this.client = this.entityCRUD.GetAll<Client>(x => x.ClientId == this.CLientId).FirstOrDefault();
            StaticConsts.ConnectedClient.ForEach(item =>
            {
                Console.WriteLine("Clien-Id: " + item.ClientId + ", ConnectedId: " + item.ConnectionId);
            });
            Console.WriteLine("Máy: " + this.CLientId);
            return base.OnConnectedAsync();
        }

        // Chỉnh lại trạng thái của static list ClientConnected
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("DisCon-Máy: " + this.CLientId);
            var connected = StaticConsts.ConnectedClient.Where(x => x.ConnectionId == this.Context.ConnectionId).FirstOrDefault();
            connected.ConnectionId = "";
            return base.OnDisconnectedAsync(exception);
        }


        // Hàm này sẽ được gọi 3 phút 1 lần. 
        // Update tài khoản, trừ tiền.
        [HubMethodName("bill")]
        public void cost()
        {
            // Lấy nhóm máy ---> giá. 
            var client = this.entityCRUD.GetAll<Client>(x => 
                x.ClientId == Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["Client-Id"])).
                FirstOrDefault();
            var clientGroup = this.entityCRUD.GetAll<GroupClient>(x => x.Id == client.ClientGroupId).
                FirstOrDefault();
            int cost = clientGroup.Price / 20;

            var account = this.entityCRUD.GetAll<Account>(x => x.Id == this.UserId).FirstOrDefault();
            account.Balance -= cost;
            var updateResult = this.entityCRUD.Update<Account, Account>(account, account).Result;


            // Khi trừ tiền, thì phát ra cho trang admin biết. Dùng IHubContext, đoạn này viết sau.
        }

        // Gửi cho server, ở bên hub khác. 
        [HubMethodName("sendToAdmin")]
        public void chatWithAdmin(string mess)
        {
            Console.WriteLine(mess);
            Console.WriteLine("Máy: " + Convert.ToInt32(this.Context.GetHttpContext().Request.Headers["Client-Id"]));
            this.adminHubs.Clients.All.SendAsync("fromClient", mess, this.Context.ConnectionId);
        }

        // Nhận tin nhắn từ hub bên server
        [HubMethodName("recieveFromAdmin")]
        public Task receiveMess(string mess, string connectionId)
        {
            return this.Clients.Clients(this.Context.ConnectionId).SendAsync("serverReply", mess);
        }

        
    }
}

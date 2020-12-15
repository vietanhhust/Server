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
    public class AdminPageHub: Hub
    {
        private EntityCRUDService entityCRUD;
        private IHubContext<ClientHub> clientHubs;
        public AdminPageHub(EntityCRUDService entityCRUD, IHubContext<ClientHub> clientHubs)
        {
            this.clientHubs = clientHubs;
            this.entityCRUD = entityCRUD; 
        }

        public override Task OnConnectedAsync()
        {
            // Thêm vào list admin đang connect
            StaticConsts.AdminConnected.Add(new AdminPageConnect
            {
                ConnectionId = this.Context.ConnectionId
            });

            StaticConsts.AdminConnected.ForEach(item=>
            {
                Console.WriteLine("Admin Connected: " + item.ConnectionId);
            });
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var adminPageFound = StaticConsts.AdminConnected.Where(x => x.ConnectionId == this.Context.ConnectionId).FirstOrDefault();
            StaticConsts.AdminConnected.Remove(adminPageFound);
            return base.OnDisconnectedAsync(exception);
        }

        [HubMethodName("sendToClient")]
        public void sendMessengrToClien(string mess, string connectionId)
        {
            // connectionId sẽ được chỉ định ( lấy từ frontend của chat admin )
            // Chọn ClientHub có connect đó, gửi về, hub Instance đó sẽ gửi lại cho Caller của
            this.clientHubs.Clients.Clients(connectionId).SendAsync("serverReply", mess);
        }

        [HubMethodName("recieveFromClient")]
        public void recieveFromClien(string mess, string connectionId)
        {
            // Gửi tin nhắn về frontend của admin, gửi cả connectionId của máy gửi đến.
            Console.WriteLine("recieveFromClient: " + mess + " ; " + connectionId);
            this.Clients.All.SendAsync("fromClient", mess, connectionId);
        }

    }
}

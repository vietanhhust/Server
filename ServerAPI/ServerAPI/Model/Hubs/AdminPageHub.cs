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
        public void sendMessengrToClien(string mess, string connectionId, long timeStamp, string adminName)
        {
            // connectionId sẽ được chỉ định ( lấy từ frontend của chat admin )
            // Chọn ClientHub có connect đó, gửi về, hub Instance đó sẽ gửi lại cho Caller của
            this.clientHubs.Clients.Clients(connectionId).SendAsync("serverReply", mess);
            this.Clients.AllExcept(this.Context.ConnectionId).SendAsync("adminToAdmin", mess, timeStamp, adminName, connectionId);
        }

        // Nhận tin nhắn từ máy trạm.
        [HubMethodName("recieveFromClient")]
        public void recieveFromClien(string mess, string connectionId)
        {
            // Gửi tin nhắn về frontend của admin, gửi cả connectionId của máy gửi đến.
            Console.WriteLine("recieveFromClient: " + mess + " ; " + connectionId);
            this.Clients.All.SendAsync("fromClient", mess, connectionId);
        }

        // Khi mât kết nối
        [HubMethodName("removeMessegeOfDisconnector")]
        public void removeMessengeFromDisconnector(string connectionId)
        {
            this.Clients.All.SendAsync("removeMessenger", connectionId);
        }

        // Khi các admin khác accept đơn gọi đồ
        [HubMethodName("acceptOrderNotifyToAdmin")]
        public void acceptOrderNotifyToOtherAdmin(string? connectionId, int orderId)
        {
            this.Clients.AllExcept(this.Context.ConnectionId).SendAsync("acceptOrderNotifyToAdmin", connectionId, orderId);
        }

        // Khi admin khác từ chối một yêu cầu
        [HubMethodName("rejectOrder")]
        public void rejectOrder(string connectionId, long timeStamp, int accountId)
        {
            this.Clients.AllExcept(this.Context.ConnectionId).SendAsync("rejectOrder", connectionId, timeStamp, accountId);
        }

        // Tạo 1 order không đến từ client, mà là admin tạo.
        [HubMethodName("createIncurredOrders")]
        public void createIncurredOrder(int? adminId,string adminName, long? timeCreated, string orderDetail ,string clienId , int? accountId, string accountName, int? id
            ) 
        {
            Console.WriteLine(adminId);
            Console.WriteLine(adminName);
            this.Clients.AllExcept(this.Context.ConnectionId).SendAsync("createIncurredOrder", adminId, timeCreated, orderDetail, clienId, accountId, adminName, accountName, id
        );
        }


        // Ra lệnh gửi ảnh máy trạm chỉ định 
        [HubMethodName("captureImage")]
        public Task captureImageClien(string connectionId)
        {
            return this.clientHubs.Clients.Clients(connectionId).SendAsync("captureImage");
        }

        // Đăng xuất máy trạm chỉ định
        [HubMethodName("logoutClient")]
        public Task logoutClient(string connectionId)
        {
            return this.clientHubs.Clients.Client(connectionId).SendAsync("logoutFromAdmin");
        }

    }
}

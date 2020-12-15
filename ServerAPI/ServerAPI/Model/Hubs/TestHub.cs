using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServerAPI.Model.Database;

namespace ServerAPI.Model.Hubs
{

    public class TestHub: Hub
    {
        private ClientManagerContext context;
        IHubContext<OtherHub> other; 

        // Inject được DbContext và HubContext để call Hub khác. 
        public TestHub(ClientManagerContext context, IHubContext<OtherHub> other)
        {
            this.context = context;
            this.other = other;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Connect from: {0}", this.Context.ConnectionId);
            Console.WriteLine(this.Context.GetHttpContext().Request.Headers[""]);
            this.Context.GetHttpContext().Request.Headers.ToList().ForEach(item =>
            {
                Console.WriteLine(item.Key + " : " + item.Value);
            });
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine("Dis from: {0}", this.Context.ConnectionId);
            await base.OnDisconnectedAsync(e);
        }

        [HubMethodName("chat")]
        public Task Chat()
        {
            
            var account = this.context.Accounts.Where(item => item.AccountName.ToLower() == "first_user").FirstOrDefault();
            //List<string> items = new List<string>();
            //this.context.Categories.ToList().ForEach(item =>
            //{
            //    items.Add(item.CategoryName);
            //});
            //// Dòng này để chứng minh là có thể DI HubContext vào HubContrustor
            //this.other.Clients.All.SendAsync("ServerReply", "Othrer");
            account.Balance -= 100;
            this.context.SaveChanges(); 
            return this.Clients.Caller.SendAsync("ServerReply", "Đã trừ tiền, tài khoản còn: " + account.Balance);
        }

    }

    public class OtherHub: Hub
    {
        [HubMethodName("chatother")]
        public Task Chat()
        {
            return this.Clients.All.SendAsync("ServerReply", "Other");
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Connect from other: {0}", this.Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine("Dis from other: {0}", this.Context.ConnectionId);
            await base.OnDisconnectedAsync(e);
        }
    }
}

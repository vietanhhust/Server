using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Database;
using ServerAPI.Model.Hubs;
using ServerAPI.Model.StaticModel;

namespace ServerAPI
{
    public class Program
    {
        // Lưu ý 1 console có thể tạo và host được nhiều hơn 2 server.
        // Vậy có thể làm tăng bảo mật bằng cách, 1 server kết nối với Port A
        // 1 server show trang quản trị ở port B, A lộ ra còn B giữ, Api của trang quản trị trỏ về B.
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            new Thread(() =>
            {
                ClientInitialize(CreateHostBuilder(args).Build()).Run();
            }).Start();
            //new Thread(() => { 
            //    ClientInitialize(CreateHostBuilder2(args).Build()).Run();
            //}).Start();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://0.0.0.0:5001");
                });

        public static IHostBuilder CreateHostBuilder2(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
                   webBuilder.UseUrls("http://0.0.0.0:5010");
               });


        // Khởi tạo danh sách client từ database
        public static IHost ClientInitialize(IHost host)
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    // Khởi tạo list client
                    var entityCRUD = services.GetRequiredService<EntityCRUDService>();
                    entityCRUD.GetAll<Client>().OrderByDescending(x => x.ClientId).ToList().ForEach(item =>
                      {
                          StaticConsts.ConnectedClient.Add(new ClientConnect
                          {
                              ClientId = item.ClientId,
                              ConnectionId = "",
                              Account = null,
                              ElapsedTime = 0, 
                              TimeLogin = 0
                          });
                      });

                    // Reset tất cả tài khoản, Account.isLogged = false
                    var accounts = entityCRUD.GetAll<Account>().ToList();
                    accounts.ForEach(item => item.IsLogged = false);
                    if (entityCRUD.UpdateRange<Account>(accounts).Result)
                    {
                        Console.WriteLine("reset tài khoản thành công");
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred.");
                }
            }

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var entityCrud = services.GetRequiredService<EntityCRUDService>();
                    var adminSignalR = services.GetRequiredService<IHubContext<AdminPageHub>>();

                    new Thread(() =>
                    {
                        while (true)
                        {
                            adminSignalR.Clients.All.SendAsync("dashboard", StaticConsts.ConnectedClient);
                            Thread.Sleep(30000);
                        }
                    }).Start(); 
                }
                catch
                {

                }
            }
            
            return host; 
        }
        }
}


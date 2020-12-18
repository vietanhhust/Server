using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Database;
using ServerAPI.Model.StaticModel;

namespace ServerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            ClientInitialize(CreateHostBuilder(args).Build()).Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://0.0.0.0:5001");
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
                              Account = null
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

            return host; 
        }
    }
}

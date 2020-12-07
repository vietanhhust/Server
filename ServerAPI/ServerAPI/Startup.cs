using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerAPI.Model.Database;
using Microsoft.AspNetCore.SignalR;
using ServerAPI.Model.Hubs;
using ServerAPI.Utilities;
using AutoMapper;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Mappings;
using ServerAPI.Controllers.Services;

namespace ServerAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Database
            // Khi dùng option Pool ( để tiêt kiệm context ), thì ClientManagerContext chỉ được phép có 1 constructor với 1 param truyền vào
            services.AddDbContextPool<ClientManagerContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ClientManagerDatabase"))
            );

            // Controller 
            services.AddControllers(option => {
                option.EnableEndpointRouting = false;
                option.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                    (err) => "Trường này không được để trống");
                option.Filters.Add(typeof(TestActionFilterAttribute));
            });

            // SignalR
            services.AddSignalR();

            // Filter 
            services.AddScoped<TestActionFilterAttribute>();

            // Mapper 
            services.AddAutoMapper(typeof(MappingProfile));

            // CRUD Entity
            services.AddScoped<EntityCRUDService>();

            // Password Service 
            services.AddScoped<PasswordService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TestHub>("/testhub");
                endpoints.MapControllers();
                endpoints.MapHub<OtherHub>("/other");
            });
        }
    }
}

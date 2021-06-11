using System.Collections.Generic;
using AbpvNext.EventBus.Cap;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace UserService
{
    [DependsOn(
        typeof(AbpvNextEventBusCapModule),
        typeof(AbpAutofacModule),
        typeof(AbpAspNetCoreMvcModule),
        typeof(AbpAspNetCoreSerilogModule),
        typeof(AbpSwashbuckleModule),
        typeof(AbpDddApplicationModule))]
    public class UserServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostingEnvironment = context.Services.GetHostingEnvironment();

            ConfigureSwaggerServices(context, configuration);

            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options
                    .ConventionalControllers
                    .Create(typeof(UserServiceModule).Assembly);
            });

            #region Cap Event Bus配置
            context.AddCapEventBus(capOptions =>
            {
                capOptions.UseSqlServer("server=localhost;user id=sa;password=123456;database=Test;");
                capOptions.UseRabbitMQ(options =>
                {
                    options.HostName = "192.168.1.100";
                    options.Port = 5672;
                    options.VirtualHost = "/";
                    options.UserName = "user";
                    options.Password = "123456";
                    options.ExchangeName = "abp.event.bus.cap.exchange";
                });
                capOptions.GroupNamePrefix = "abp";
                capOptions.DefaultGroupName = "event.bus.cap.group";
                capOptions.ConsumerThreadCount = 2;
                capOptions.ProducerThreadCount = 2;
                capOptions.FailedRetryCount = 3;
                capOptions.UseDashboard();
            });

            Configure<AbpCapEventBusOptions>(options =>
            {
                options.IsEnabledSameEventMultiGroup = true;
            });
            #endregion
        }

        private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "User Api",
                        Description = "用户服务api"
                    });
                    options.DocInclusionPredicate((docName, description) => true);
                });
        }


        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseAbpSerilogEnrichers();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "User API");
            });
            app.UseConfiguredEndpoints();
        }
    }
}
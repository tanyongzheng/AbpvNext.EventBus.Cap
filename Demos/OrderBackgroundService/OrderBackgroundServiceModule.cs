using AbpvNext.EventBus.Cap;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using AbpvNext.EventBus.Cap;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Volo.Abp;
using DotNetCore.CAP.TopicExtensions;

namespace OrderBackgroundService
{
    [DependsOn(
        typeof(AbpvNextEventBusCapModule),
        typeof(AbpAutofacModule))]
    public class OrderBackgroundServiceModule : AbpModule
    {

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostEnvironment = context.Services.GetSingletonInstance<IHostEnvironment>();

            #region Cap Event Bus配置
            context.Services.AddCapTopicExtension(options =>
            {
                /*
                options.UnsubscribedTopics = new List<string>()
                {
                    "Demo.User.AddUser"
                };
                options.UnsubscribedTopics = new List<string>()
                {
                    "Demo.UpdateUserAddress"
                };
                */
            });
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
                capOptions.DefaultGroupName = "event.bus.cap.group.order.background";
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

            context.Services.AddHostedService<OrderBackgroundHostedService>();
        }

        /*
        private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "Order Api",
                        Description = "订单服务api"
                    });
                    options.DocInclusionPredicate((docName, description) => true);
                });
        }
        */

        /*
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
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API");
            });
            app.UseConfiguredEndpoints();
        }
        */
    }
}
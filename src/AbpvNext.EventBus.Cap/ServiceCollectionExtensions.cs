using System;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;

namespace AbpvNext.EventBus.Cap
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 增加Cap分布式事件总线
        /// </summary>
        /// <param name="context"></param>
        /// <param name="capAction"></param>
        /// <returns></returns>
        public static ServiceConfigurationContext AddCapEventBus(this ServiceConfigurationContext context, Action<CapOptions> capAction)
        {
            context.Services.AddCap(capAction);
            // 替换cap默认的消费者服务查找器
            context.Services.AddSingleton<IConsumerServiceSelector, AbpConsumerServiceSelector>();
            context.Services.AddSingleton<IDistributedEventBus, CapDistributedEventBus>();
            return context;
        }
    }
}
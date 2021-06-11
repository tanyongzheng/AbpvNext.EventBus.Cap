using System;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCore.CAP.TopicExtensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Cap主题拓展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddCapTopicExtension(this IServiceCollection services, Action<CapTopicExtensionOptions> setupAction)
        {
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.Configure(setupAction);
            services.AddSingleton<FilterTopicService>();
            // 替换cap默认的消费者服务查找器
            services.AddSingleton<IConsumerServiceSelector, TopicExtensionConsumerServiceSelector>();
            return services;
        }
    }
}
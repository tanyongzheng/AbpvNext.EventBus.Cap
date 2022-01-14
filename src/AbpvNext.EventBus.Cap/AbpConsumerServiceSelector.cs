using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.TopicExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;

namespace AbpvNext.EventBus.Cap
{
    /// <summary>
    /// 消费者查找器
    /// </summary>
    [Dependency(ServiceLifetime.Singleton, ReplaceServices = true)]
    [ExposeServices(typeof(IConsumerServiceSelector), typeof(AbpConsumerServiceSelector))]
    public class AbpConsumerServiceSelector : TopicExtensionConsumerServiceSelector
    {
        /// <summary>
        /// CAP配置
        /// </summary>
        private readonly CapOptions _capOptions;

        /// <summary>
        /// Abp分布式事件配置
        /// </summary>
        private readonly AbpDistributedEventBusOptions _abpDistributedEventBusOptions;

        /// <summary>
        /// 服务提供者
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Cap Event Bus配置
        /// </summary>
        private readonly AbpCapEventBusOptions _eventBusOptions;


        private readonly FilterTopicService _filterTopicService;

        /// <summary>
        /// Creates a new <see cref="T:DotNetCore.CAP.Internal.ConsumerServiceSelector" />.
        /// </summary>
        public AbpConsumerServiceSelector(
            IServiceProvider serviceProvider,
            IOptions<CapOptions> capOptions,
            IOptions<AbpDistributedEventBusOptions> distributedEventBusOptions,
            IOptions<AbpCapEventBusOptions> eventBusOptions,
            FilterTopicService filterTopicService
            ) : base(serviceProvider)
        {
            _capOptions = capOptions.Value;
            _serviceProvider = serviceProvider;
            _abpDistributedEventBusOptions = distributedEventBusOptions.Value;
            _eventBusOptions = eventBusOptions.Value;
            _filterTopicService = filterTopicService;
        }

        /// <summary>
        /// 查找Abp框架EventHandler的消费者集合
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        protected override IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromInterfaceTypes(IServiceProvider provider)
        {
            // Cap所有的消费者(实现ICapSubscribe接口)
            var capConsumerExecutorDescriptorList = base.FindConsumersFromInterfaceTypes(provider).ToList();

            // Abp的消费者
            // var abpConsumerExecutorDescriptorList = new List<ConsumerExecutorDescriptor>();

            // Abp 所有事件处理类型
            var abpEventHandlerTypes = _abpDistributedEventBusOptions.Handlers;

            foreach (var abpEventHandlerType in abpEventHandlerTypes)
            {
                // Abp 事件处理接口
                var abpEventHandlerInterfaces = abpEventHandlerType.GetInterfaces();
                foreach (var abpEventHandlerInterface in abpEventHandlerInterfaces)
                {
                    if (!typeof(IEventHandler).GetTypeInfo().IsAssignableFrom(abpEventHandlerInterface))
                    {
                        continue;
                    }

                    // 从泛型EventHandler接口中获取泛型类型（abp中的Eto类型）
                    var genericArgs = abpEventHandlerInterface.GetGenericArguments();

                    if (genericArgs.Length != 1)
                    {
                        continue;
                    }
                    // Event Transfer Object Type
                    var eventTransferObjectsType = genericArgs[0];

                    // Abp 事件处理消费者
                    var abpConsumerExecutorDescriptors = GetAbpEventHandlerDescription(eventTransferObjectsType, abpEventHandlerType);
                    capConsumerExecutorDescriptorList.AddRange(abpConsumerExecutorDescriptors);

                    //Subscribe(genericArgs[0], new IocEventHandlerFactory(ServiceScopeFactory, handler));
                    
                }
            }

            // 同事件名的消费者数，如果同一事件处理者（分组相同）有多个实例（譬如订单服务和订单后台服务都订阅处理了同一个ETO）
            // 如都需要处理该事件（RabbitMq中为路由键），则需要开启多个分组(RabbitMq中为多个队列)
            // 也会修改直接使用Cap的分组（防止直接Cap的消费者被忽略）
            if (_eventBusOptions.IsEnabledSameEventMultiGroup)
            {
                // 判断当前启动的Host实列是否订阅多个ETO的Handler（相同路由键）
                // 根据主题名（路由键）组合
                //var capConsumerExecutorDescriptorGroups = capConsumerExecutorDescriptorList.GroupBy(x => x.Attribute.Name);
                var capConsumerExecutorDescriptorGroups = capConsumerExecutorDescriptorList.GroupBy(x => x.Attribute);

                foreach (var capConsumerExecutorDescriptorGroup in capConsumerExecutorDescriptorGroups)
                {
                    var sameNamecapConsumerExecutorDescriptors=capConsumerExecutorDescriptorGroup.ToList();
                    var sameEventNameConsumerCount = sameNamecapConsumerExecutorDescriptors.Count;
                    if (sameEventNameConsumerCount == 1)
                    {
                        continue;
                    }
                    for(var i=0; i<sameEventNameConsumerCount; i++)
                    {
                        //忽略一个，保持与CAP的主题名相同
                        if (i == 0) continue;

                        sameNamecapConsumerExecutorDescriptors[i].Attribute.Group +=
                            $".{_eventBusOptions.SameEventMultiGroupIndexPrefix}{sameEventNameConsumerCount}_{i}";
                    }
                }
            }
            return _filterTopicService.FilterTopics(capConsumerExecutorDescriptorList);
        }

        /// <summary>
        /// 获取Abp事件处理器集合
        /// </summary>
        /// <param name="eventDataType">事件对象类型</param>
        /// <param name="typeInfo">事件执行的Handler类型</param>
        /// <returns></returns>
        protected virtual IEnumerable<ConsumerExecutorDescriptor> GetAbpEventHandlerDescription(Type eventDataType, Type typeInfo)
        {
            var serviceTypeInfo = typeof(IDistributedEventHandler<>).MakeGenericType(eventDataType);
            // 获取事件执行方法
            var method = typeInfo.GetMethod(
                    nameof(IDistributedEventHandler<object>.HandleEventAsync),
                    new[] { eventDataType }
                );

            if (method == null)
            {
                yield break;
            }

            // 获取Abp的事件名
            var eventName = EventNameAttribute.GetNameOrDefault(eventDataType);

            // 获取Cap的Topic特性信息
            var topicAttr = method.GetCustomAttributes<TopicAttribute>(true);
            var topicAttributes = topicAttr.ToList();

            //topicAttributes.Add(new CapSubscribeAttribute(eventName));

            // 没有与Cap同名的Topic则加入Abp的事件订阅
            if (topicAttributes.Count == 0)
            {
                topicAttributes.Add(new CapSubscribeAttribute(eventName));
            }

            foreach (var attr in topicAttributes)
            {
                // Group为队列名，多个Abp的Event Transfer Objects的EventName特性会变成Topic or exchange route key name.
                // 队列名使用CAP的DefaultGroupName +Abp的EventName（Rabbitmq为队列名，发送消息的EventName为Routing key）
                // SetSubscribeAttribute(consumerExecutorDescriptor.Attribute);
                // 启用Cap的分组名作为队列前缀
                SetSubscribeAttribute(attr);

                var parameters = method.GetParameters()
                    .Select(parameter => new ParameterDescriptor
                    {
                        Name = parameter.Name,
                        ParameterType = parameter.ParameterType,
                        IsFromCap = parameter.GetCustomAttributes(typeof(FromCapAttribute)).Any()
                    }).ToList();

                yield return InitDescriptor(attr, method, typeInfo.GetTypeInfo(), serviceTypeInfo.GetTypeInfo(), parameters);
            }
        }

        private static ConsumerExecutorDescriptor InitDescriptor(
            TopicAttribute attr,
            MethodInfo methodInfo,
            TypeInfo implType,
            TypeInfo serviceTypeInfo,
            IList<ParameterDescriptor> parameters)
        {
            var descriptor = new ConsumerExecutorDescriptor
            {
                Attribute = attr,
                MethodInfo = methodInfo,
                ImplTypeInfo = implType,
                ServiceTypeInfo = serviceTypeInfo,
                Parameters = parameters
            };

            return descriptor;
        }


        protected IEnumerable<ConsumerExecutorDescriptor> GetTopicAttributesDescription(TypeInfo typeInfo, TypeInfo serviceTypeInfo = null)
        {
            var topicClassAttribute = typeInfo.GetCustomAttribute<TopicAttribute>(true);

            foreach (var method in typeInfo.GetRuntimeMethods())
            {
                var topicMethodAttributes = method.GetCustomAttributes<TopicAttribute>(true);

                // Ignore partial attributes when no topic attribute is defined on class.
                if (topicClassAttribute is null)
                {
                    topicMethodAttributes = topicMethodAttributes.Where(x => !x.IsPartial);
                }

                if (!topicMethodAttributes.Any())
                {
                    continue;
                }

                foreach (var attr in topicMethodAttributes)
                {
                    SetSubscribeAttribute(attr);

                    var parameters = method.GetParameters()
                        .Select(parameter => new ParameterDescriptor
                        {
                            Name = parameter.Name,
                            ParameterType = parameter.ParameterType,
                            IsFromCap = parameter.GetCustomAttributes(typeof(FromCapAttribute)).Any()
                        }).ToList();

                    yield return InitDescriptor(attr, method, typeInfo, serviceTypeInfo, parameters, topicClassAttribute);
                }
            }
        }


        private ConsumerExecutorDescriptor InitDescriptor(
            TopicAttribute attr,
            MethodInfo methodInfo,
            TypeInfo implType,
            TypeInfo serviceTypeInfo,
            IList<ParameterDescriptor> parameters,
            TopicAttribute classAttr = null)
        {
            var descriptor = new ConsumerExecutorDescriptor
            {
                Attribute = attr,
                ClassAttribute = classAttr,
                MethodInfo = methodInfo,
                ImplTypeInfo = implType,
                ServiceTypeInfo = serviceTypeInfo,
                Parameters = parameters,
                TopicNamePrefix = _capOptions.TopicNamePrefix
            };

            return descriptor;
        }


    }
}
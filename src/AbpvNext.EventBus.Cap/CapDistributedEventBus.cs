using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace AbpvNext.EventBus.Cap
{
    [Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(IDistributedEventBus), typeof(CapDistributedEventBus))]
    public class CapDistributedEventBus : EventBusBase, IDistributedEventBus, ISingletonDependency
    {
        /// <summary>
        /// Abp分布式事件总线配置
        /// </summary>
        protected AbpDistributedEventBusOptions AbpDistributedEventBusOptions { get; }

        /// <summary>
        /// CAP消息发布接口
        /// </summary>
        protected ICapPublisher CapPublisher { get; }

        /// <summary>
        /// 事件处理器工厂字典
        /// </summary>
        protected ConcurrentDictionary<Type, List<IEventHandlerFactory>> HandlerFactories { get; }

        /// <summary>
        /// 事件类型字典
        /// </summary>
        protected ConcurrentDictionary<string, Type> EventTypes { get; }

        
        public CapDistributedEventBus(
            IOptions<AbpDistributedEventBusOptions> distributedEventBusOptions,
            ICapPublisher capPublisher,
            IServiceScopeFactory serviceScopeFactory, 
            ICurrentTenant currentTenant,
            IUnitOfWorkManager unitOfWorkManager,
            IEventHandlerInvoker eventHandlerInvoker): base(serviceScopeFactory, currentTenant, unitOfWorkManager, eventHandlerInvoker)
        {
            AbpDistributedEventBusOptions = distributedEventBusOptions.Value;
            CapPublisher = capPublisher; 
            HandlerFactories = new ConcurrentDictionary<Type, List<IEventHandlerFactory>>();
            EventTypes = new ConcurrentDictionary<string, Type>();
        }
        

        public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
        {
            // return Subscribe(eventType, factory);
            // 订阅由自定义的消费者服务选择器ConsumerServiceSelector实现
            throw new NotImplementedException();
        }

        public override void Unsubscribe<TEvent>(Func<TEvent, Task> action)
        {
            Check.NotNull(action, nameof(action));

            GetOrCreateHandlerFactories(typeof(TEvent))
                .Locking(factories =>
                {
                    factories.RemoveAll(
                        factory =>
                        {
                            var singleInstanceFactory = factory as SingleInstanceHandlerFactory;
                            if (singleInstanceFactory == null)
                            {
                                return false;
                            }

                            var actionHandler = singleInstanceFactory.HandlerInstance as ActionEventHandler<TEvent>;
                            if (actionHandler == null)
                            {
                                return false;
                            }

                            return actionHandler.Action == action;
                        });
                });
        }

        public override void Unsubscribe(Type eventType, IEventHandler handler)
        {
            GetOrCreateHandlerFactories(eventType)
                .Locking(factories =>
                {
                    factories.RemoveAll(
                        factory =>
                            factory is SingleInstanceHandlerFactory &&
                            (factory as SingleInstanceHandlerFactory).HandlerInstance == handler
                    );
                });
        }

        public override void Unsubscribe(Type eventType, IEventHandlerFactory factory)
        {
            GetOrCreateHandlerFactories(eventType).Locking(factories => factories.Remove(factory));
        }

        public override void UnsubscribeAll(Type eventType)
        {
            GetOrCreateHandlerFactories(eventType).Locking(factories => factories.Clear());
        }

        public IDisposable Subscribe<TEvent>(IDistributedEventHandler<TEvent> handler) where TEvent : class
        {
            return Subscribe(typeof(TEvent), handler);
        }

        /*
        public override async Task PublishAsync(Type eventType, object eventData)
        {
            var eventName = EventNameAttribute.GetNameOrDefault(eventType);
            await CapPublisher.PublishAsync(eventName, eventData);
        }
        */

        public virtual async Task PublishAsync(Type eventType, object eventData, bool onUnitOfWorkComplete = true, bool useOutbox = true)
        {

            if (onUnitOfWorkComplete && UnitOfWorkManager.Current != null)
            {
                AddToUnitOfWork(
                    UnitOfWorkManager.Current,
                    new UnitOfWorkEventRecord(eventType, eventData, EventOrderGenerator.GetNext(), useOutbox)
                );
                return;
            }

            // TODO: 如使用发件箱，转到CAP的事务工作单元（使用CAP自带的发件箱）
            if (useOutbox && UnitOfWorkManager.Current != null)
            {
                // 获取Cap的工作单元
                // var capTransaction = CapPublisher.Transaction.Value;

                await PublishToEventBusAsync(eventType, eventData);
                return;
            }

            await PublishToEventBusAsync(eventType, eventData);
        }

        public virtual Task PublishAsync<TEvent>(TEvent eventData, bool onUnitOfWorkComplete = true, bool useOutbox = true) where TEvent : class
        {
            return PublishAsync(typeof(TEvent), eventData, onUnitOfWorkComplete, useOutbox);
        }


        protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType)
        {
            var handlerFactoryList = new List<EventTypeWithEventHandlerFactories>();

            foreach (var handlerFactory in HandlerFactories.Where(hf => ShouldTriggerEventForHandler(eventType, hf.Key)))
            {
                handlerFactoryList.Add(new EventTypeWithEventHandlerFactories(handlerFactory.Key, handlerFactory.Value));
            }

            return handlerFactoryList.ToArray();
        }

        private List<IEventHandlerFactory> GetOrCreateHandlerFactories(Type eventType)
        {
            return HandlerFactories.GetOrAdd(
                eventType,
                type =>
                {
                    var eventName = EventNameAttribute.GetNameOrDefault(type);
                    EventTypes[eventName] = type;
                    return new List<IEventHandlerFactory>();
                }
            );
        }

        private static bool ShouldTriggerEventForHandler(Type targetEventType, Type handlerEventType)
        {
            //Should trigger same type
            if (handlerEventType == targetEventType)
            {
                return true;
            }

            //TODO: Support inheritance? But it does not support on subscription to RabbitMq!
            //Should trigger for inherited types
            if (handlerEventType.IsAssignableFrom(targetEventType))
            {
                return true;
            }

            return false;
        }

        protected override async Task PublishToEventBusAsync(Type eventType, object eventData)
        {
            var eventName = EventNameAttribute.GetNameOrDefault(eventType);
            await CapPublisher.PublishAsync(eventName, eventData);
        }

        protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord)
        {
            unitOfWork.AddOrReplaceDistributedEvent(eventRecord);
        }

    }
}
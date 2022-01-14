using EventTransferObjects;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace OrderService.EventHandlers
{

    public class UpdateUserAddress4EventHandler : IDistributedEventHandler<UpdateUserAddressEto>, ITransientDependency
    {

        //public IAbpLazyServiceProvider LazyServiceProvider { get; set; }
        //protected ILogger Logger => LazyServiceProvider.LazyGetService<ILogger>(provider => LoggerFactory?.CreateLogger(GetType().FullName) ?? NullLogger.Instance);

        public UpdateUserAddress4EventHandler()
        {

        }

        public async Task HandleEventAsync(UpdateUserAddressEto eventData)
        {
            var address = eventData.Address;
            // Logger.LogInformation("收到更新用户地址事件："+eventData.Address);
        }
    }
}

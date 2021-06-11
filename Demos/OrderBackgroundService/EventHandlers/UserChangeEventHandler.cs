using System.Threading.Tasks;
using EventTransferObjects;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace OrderBackgroundService.EventHandlers
{
    public class UserChangeEventHandler : IDistributedEventHandler<UpdateUserAddressEto>, ITransientDependency
    {
        public UserChangeEventHandler()
        {

        }

        public async Task HandleEventAsync(UpdateUserAddressEto eventData)
        {
            var address = eventData.Address;
            // Logger.LogInformation("收到更新用户地址事件："+eventData.Address);
        }
    }
}
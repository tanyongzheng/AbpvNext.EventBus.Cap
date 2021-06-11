using EventTransferObjects;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;


namespace OrderService.EventHandlers
{
    //[NonController]
    //[ApiExplorerSettings(IgnoreApi = true)]
    //[RemoteService(IsMetadataEnabled = false)]
    public class UpdateUserAddressEventHandler : IDistributedEventHandler<UpdateUserAddressEto>, ITransientDependency
    {

        //public IAbpLazyServiceProvider LazyServiceProvider { get; set; }
        //protected ILogger Logger => LazyServiceProvider.LazyGetService<ILogger>(provider => LoggerFactory?.CreateLogger(GetType().FullName) ?? NullLogger.Instance);

        public UpdateUserAddressEventHandler()
        {

        }

        public async Task HandleEventAsync(UpdateUserAddressEto eventData)
        {
            var address = eventData.Address;
            // Logger.LogInformation("收到更新用户地址事件："+eventData.Address);
        }
    }
}
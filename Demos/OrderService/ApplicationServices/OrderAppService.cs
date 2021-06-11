using System.Threading.Tasks;
using EventTransferObjects;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Distributed;

namespace OrderService.ApplicationServices
{
    public class OrderAppService : ApplicationService
    {
        private readonly IDistributedEventBus _distributedEventBus;

        public OrderAppService(IDistributedEventBus distributedEventBus)
        {
            _distributedEventBus = distributedEventBus;
        }

        public async Task<string> GetOrder()
        {
            return "123";
        }
    }
}
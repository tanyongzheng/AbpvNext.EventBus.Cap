using System.Threading.Tasks;
using EventTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Distributed;

namespace UserService.ApplicationServices
{
    public class UserAppService:ApplicationService
    {
        private readonly IDistributedEventBus _distributedEventBus;

        public UserAppService(IDistributedEventBus distributedEventBus)
        {
            _distributedEventBus = distributedEventBus;
        }


        public async Task<string> UpdateAddress(string address)
        {
            await _distributedEventBus.PublishAsync(new UpdateUserAddressEto()
            {
                Address = address
            });
            return address;
        }
    }
}
using System;
using Volo.Abp.EventBus;

namespace EventTransferObjects
{
    [EventName("Demo.UpdateUserAddress")]
    public class UpdateUserAddressEto
    {
        public string Address { get; set; }
    }
}

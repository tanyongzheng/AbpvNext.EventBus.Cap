using DotNetCore.CAP;

namespace AbpvNext.EventBus.Cap
{
    public class AbpCapEventBusOptions
    {
        /// <summary>
        /// 是否启同一事件多个分组
        /// 多个队列订阅同一事件
        /// </summary>
        public bool IsEnabledSameEventMultiGroup { get; set; }

        /// <summary>
        /// 多分组名序号前缀
        /// 如queue,生成对应分组结尾为.queue0，.queue1
        /// </summary>
        public string SameEventMultiGroupIndexPrefix { get; set; }

    }
}
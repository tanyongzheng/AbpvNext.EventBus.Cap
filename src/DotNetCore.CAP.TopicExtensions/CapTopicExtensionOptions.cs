using System.Collections.Generic;

namespace DotNetCore.CAP.TopicExtensions
{
    public class CapTopicExtensionOptions
    {
        /// <summary>
        /// 已订阅的主题，与未订阅的主题互斥
        /// 该参数不为空则只保留配置的已订阅的主题，其他的主题全部移除
        /// </summary>
        public List<string> SubscribedTopics { get; set; }

        /// <summary>
        /// 未已订阅的主题，与已订阅的主题互斥
        /// 该参数不为空则移除未订阅的主题
        /// </summary>
        public List<string> UnsubscribedTopics { get; set; }

    }
}
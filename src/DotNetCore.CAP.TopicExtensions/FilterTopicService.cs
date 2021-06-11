using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.TopicExtensions
{
    public class FilterTopicService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CapOptions _capOptions;
        private readonly CapTopicExtensionOptions _capTopicExtensionOptions;

        public FilterTopicService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _capOptions = serviceProvider.GetService<IOptions<CapOptions>>().Value;
            IOptions<CapTopicExtensionOptions> capTopicExtensionOptions = serviceProvider.GetService<IOptions<CapTopicExtensionOptions>>();
            if (capTopicExtensionOptions == null || capTopicExtensionOptions.Value == null)
            {
                throw new ArgumentNullException($"Please configure {nameof(CapTopicExtensionOptions)}");
            }

            _capTopicExtensionOptions = capTopicExtensionOptions.Value;
            if (_capTopicExtensionOptions.SubscribedTopics != null &&
                _capTopicExtensionOptions.SubscribedTopics.Count > 0 &&
                _capTopicExtensionOptions.UnsubscribedTopics != null &&
                _capTopicExtensionOptions.UnsubscribedTopics.Count > 0
            )
            {
                throw new ArgumentNullException($"{nameof(CapTopicExtensionOptions)}的参数{_capTopicExtensionOptions.SubscribedTopics}和{_capTopicExtensionOptions.UnsubscribedTopics}只能配置一个！");
            }

            if (_capTopicExtensionOptions.SubscribedTopics != null &&
                _capTopicExtensionOptions.SubscribedTopics.Count > 0)
            {
                _capTopicExtensionOptions.SubscribedTopics =
                    _capTopicExtensionOptions.SubscribedTopics.Distinct().ToList();
            }

            if (_capTopicExtensionOptions.UnsubscribedTopics != null &&
                _capTopicExtensionOptions.UnsubscribedTopics.Count > 0)
            {
                _capTopicExtensionOptions.UnsubscribedTopics =
                    _capTopicExtensionOptions.UnsubscribedTopics.Distinct().ToList();
            }
        }


        /// <summary>
        /// 筛选主题
        /// </summary>
        /// <param name="executorDescriptorList"></param>
        /// <returns></returns>
        public List<ConsumerExecutorDescriptor> FilterTopics(List<ConsumerExecutorDescriptor> executorDescriptorList)
        {
            if (executorDescriptorList == null || executorDescriptorList.Count == 0)
            {
                return executorDescriptorList;
            }

            if (_capTopicExtensionOptions.SubscribedTopics != null && _capTopicExtensionOptions.SubscribedTopics.Count > 0)
            {
                var subscribedTopicExecutorDescriptorList = new List<ConsumerExecutorDescriptor>();
                foreach (var item in _capTopicExtensionOptions.SubscribedTopics)
                {
                    var topicName = item;
                    if (!string.IsNullOrEmpty(_capOptions.TopicNamePrefix))
                    {
                        topicName = $"{_capOptions.TopicNamePrefix}.{topicName}";
                    }
                    var subscribedTopicExecutorDescriptors = executorDescriptorList.FindAll(x => x.TopicName == topicName);
                    if (subscribedTopicExecutorDescriptors.Count > 0)
                    {
                        subscribedTopicExecutorDescriptorList.AddRange(subscribedTopicExecutorDescriptors);
                    }
                }

                executorDescriptorList = subscribedTopicExecutorDescriptorList;
            }

            if (_capTopicExtensionOptions.UnsubscribedTopics != null && _capTopicExtensionOptions.UnsubscribedTopics.Count > 0)
            {
                foreach (var item in _capTopicExtensionOptions.UnsubscribedTopics)
                {
                    var topicName = item;
                    if (!string.IsNullOrEmpty(_capOptions.TopicNamePrefix))
                    {
                        topicName = $"{_capOptions.TopicNamePrefix}.{topicName}";
                    }
                    executorDescriptorList.RemoveAll(x => x.TopicName == topicName);
                }
            }

            return executorDescriptorList;
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetCore.CAP.TopicExtensions
{
    public partial class TopicExtensionConsumerServiceSelector : ConsumerServiceSelector
    {
        private readonly CapOptions _capOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly FilterTopicService _filterTopicService;

        public TopicExtensionConsumerServiceSelector(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _capOptions = serviceProvider.GetService<IOptions<CapOptions>>().Value;
            _filterTopicService = serviceProvider.GetService<FilterTopicService>();
        }
        
        protected override IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromInterfaceTypes(
            IServiceProvider provider)
        {
            var executorDescriptorList = base.FindConsumersFromInterfaceTypes(provider).ToList();
            return _filterTopicService.FilterTopics(executorDescriptorList);
        }

        protected override IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromControllerTypes()
        {
            var executorDescriptorList = base.FindConsumersFromControllerTypes().ToList();

            return _filterTopicService.FilterTopics(executorDescriptorList);
        }


    }
}
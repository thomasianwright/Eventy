using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Contexts;
using Eventy.Abstractions.Events.Contracts;
using Eventy.Abstractions.IoC.Services;
using Eventy.Abstractions.Transports.Services;
using Microsoft.Extensions.Logging;

namespace Eventy.Core.Events.Consumers
{
    public abstract class EventConsumerHandler<TProvider> where TProvider : ITransportProvider
    {
        protected EventConsumerHandler(ILogger logger, TProvider provider, IServiceResolver serviceResolver,
            Type consumerType, Type eventType)
        {
            Logger = logger;
            Provider = provider;
            ServiceResolver = serviceResolver;
            ConsumerType = consumerType;
            EventType = eventType;
            Topology = provider.EventTopologies[eventType];
        }

        protected ILogger Logger { get; set; }
        protected IServiceResolver ServiceResolver { get; set; }
        protected Type ConsumerType { get; set; }
        protected Type EventType { get; set; }
        protected TProvider Provider { get; set; }
        public IEventTopology Topology { get; set; }

        public abstract Task StartAsync(CancellationToken cancellationToken = default);

        public abstract Task StopAsync(CancellationToken cancellationToken = default);

        protected abstract IEventContext CreateEventContext(IEvent @event, IDictionary<string, object> headers);
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.IoC.Services;
using Eventy.Logging.Services;
using Eventy.Transports.Services;

namespace Eventy.Events.Consumers
{
    public abstract class EventConsumerHandler<TProvider> where TProvider : ITransportProvider
    {
        protected EventConsumerHandler(IEventLogger logger, TProvider provider, IServiceResolver serviceResolver,
            Type consumerType, Type eventType)
        {
            Logger = logger;
            Provider = provider;
            ServiceResolver = serviceResolver;
            ConsumerType = consumerType;
            EventType = eventType;
            Topology = provider.EventTopologies[eventType];
        }

        protected IEventLogger Logger { get; set; }
        protected IServiceResolver ServiceResolver { get; set; }
        protected Type ConsumerType { get; set; }
        protected Type EventType { get; set; }
        protected TProvider Provider { get; set; }
        protected IEventTopology Topology { get; set; }

        public abstract Task StartAsync(CancellationToken cancellationToken = default);

        public abstract Task StopAsync(CancellationToken cancellationToken = default);

        protected abstract IEventContext CreateEventContext(IEvent @event, IDictionary<string, object> headers, Guid? messageId = null);
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Clients;
using Eventy.Abstractions.Events.Consumers;
using Eventy.Abstractions.Events.Contracts;
using Eventy.Abstractions.Events.Encoders;
using Eventy.Abstractions.IoC.Services;
using Eventy.Core.Events.Attributes;
using Eventy.Transports.RabbitMQ.Clients;
using Eventy.Transports.RabbitMQ.Consumers;
using Eventy.Transports.RabbitMQ.Contracts;
using FluentResults;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Eventy.Transports.RabbitMQ
{
    public class RabbitMqTransportProvider : IRabbitMqTransportProvider
    {
        private readonly IList<Type> _consumerTypes = new List<Type>();

        private readonly IList<Type> _eventTypes = new List<Type>();
        private CancellationTokenSource _cts;
        private TaskFactory _taskFactory;


        public RabbitMqTransportProvider(ILogger<IRabbitMqTransportProvider> logger, IServiceResolver serviceResolver,
            IEventEncoder encoder, IConnection connection)
        {
            Encoder = encoder;
            Connection = connection;
            Logger = logger;
            ServiceResolver = serviceResolver;
        }

        protected ILogger Logger { get; set; }
        protected IServiceResolver ServiceResolver { get; set; }

        internal IList<Type> ConsumerTypes { get; } = new List<Type>();

        internal ConcurrentDictionary<Type, RabbitMqEventConsumerHandler> Consumers { get; } =
            new ConcurrentDictionary<Type, RabbitMqEventConsumerHandler>();

        private ConcurrentDictionary<Type, IRequestClient> RequestClients { get; } =
            new ConcurrentDictionary<Type, IRequestClient>();

        public IEventEncoder Encoder { get; }

        public Task<Result> Start()
        {
            foreach (var eventType in _eventTypes)
            {
                var topologyAttribute = eventType.GetCustomAttributes(typeof(EventTopologyAttribute), false)
                    .FirstOrDefault() as EventTopologyAttribute;

                if (topologyAttribute == null)
                    throw new Exception($"Event type {eventType.Name} does not have EventTopologyAttribute");

                EventTopologies.TryAdd(eventType, topologyAttribute);
            }

            _taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning);

            foreach (var type in _consumerTypes)
            {
                var eventType = type.GetInterfaces()
                    .FirstOrDefault(x =>
                        x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IConsumer<>) &&
                        x.GetGenericArguments().Any())
                    ?.GetGenericArguments().First();

                var consumer =
                    new RabbitMqEventConsumerHandler(Logger, this, ServiceResolver, type, eventType, Encoder);

                Consumers.TryAdd(type, consumer);

                consumer.StartAsync();
            }

            return Task.FromResult(Result.Ok());
        }

        public Result Stop()
        {
            Consumers.Clear();
            return Result.Ok();
        }

        public IConnection Connection { get; }

        public void Dispose()
        {
            Connection?.Dispose(); 
        }

        public Task<IResponse> RequestAsync<T>(T @event) where T : IEvent, ICorrelatedBy<Guid>
        {
            if (@event.CorrelationId == Guid.Empty)
                @event.CorrelationId = Guid.NewGuid();
            
            var requestClient = GetRequestClient<T>();

            return requestClient.RequestAsync<T>(@event);
        }

        public Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : IEvent, ICorrelatedBy<Guid>
        {
            var model = Connection.CreateModel();
            var topology = EventTopologies[@event.GetType()];
            var body = Encoder.Encode(@event);

            var properties = model.CreateBasicProperties();
            properties.CorrelationId = @event.CorrelationId.ToString();
            properties.ContentType = "application/json";

            model.BasicPublish(topology.ExchangeName, topology.RoutingKey, properties, body);

            return Task.CompletedTask;
        }

        public Task SendAsync<T>(T @event) where T : IEvent, ICorrelatedBy<Guid>
        {
            var model = Connection.CreateModel();
            var topology = EventTopologies[@event.GetType()];
            var body = Encoder.Encode(@event);

            var properties = model.CreateBasicProperties();
            properties.CorrelationId = @event.CorrelationId.ToString();
            properties.ContentType = "application/json";
            model.BasicPublish(
                topology.ExchangeName,
                topology.RoutingKey,
                properties,
                body
            );

            return Task.CompletedTask;
        }

        public string Name => "RabbitMQ";

        public ConcurrentDictionary<Type, IEventTopology> EventTopologies { get; } =
            new ConcurrentDictionary<Type, IEventTopology>();

        public Result AddEventTypes(params Type[] eventTypes)
        {
            var result = Result.Ok();

            foreach (var eventType in eventTypes)
            {
                if (_eventTypes.Contains(eventType))
                {
                    result.WithError($"Event type {eventType.Name} already added");
                    continue;
                }

                _eventTypes.Add(eventType);
            }

            return result;
        }

        public Result AddConsumers(params Type[] consumers)
        {
            var result = Result.Ok();

            foreach (var consumer in consumers)
            {
                if (_consumerTypes.Contains(consumer))
                {
                    result.WithError($"Consumer type {consumer.Name} already added");
                    continue;
                }

                _consumerTypes.Add(consumer);
            }

            return result;
        }

        private RabbitMqRequestClient<T> GetRequestClient<T>() where T : IEvent
        {
            if (RequestClients.TryGetValue(typeof(T), out var client)) return (RabbitMqRequestClient<T>)client;

            var requestClient = new RabbitMqRequestClient<T>(this);

            RequestClients.TryAdd(typeof(T), requestClient);

            return requestClient;
        }
    }
}
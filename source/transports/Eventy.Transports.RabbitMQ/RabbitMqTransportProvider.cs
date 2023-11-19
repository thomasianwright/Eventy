using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Attributes;
using Eventy.Events.Clients;
using Eventy.Events.Consumers;
using Eventy.Events.Contracts;
using Eventy.Events.Encoders;
using Eventy.IoC.Services;
using Eventy.Logging.Services;
using Eventy.RabbitMQ.Clients;
using Eventy.RabbitMQ.Consumers;
using Eventy.RabbitMQ.Contracts;
using FluentResults;
using RabbitMQ.Client;

namespace Eventy.RabbitMQ
{
    public class RabbitMqTransportProvider : IRabbitMqTransportProvider
    {
        public string Name => "RabbitMQ";

        public ConcurrentDictionary<Type, IEventTopology> EventTopologies { get; } =
            new ConcurrentDictionary<Type, IEventTopology>();

        public IEventEncoder Encoder { get; }

        public IConnection Connection { get; }

        private IEventLogger Logger { get; set; }
        private IServiceResolver ServiceResolver { get; set; }

        private ConcurrentDictionary<Type, RabbitMqEventConsumerHandler> Consumers { get; } =
            new ConcurrentDictionary<Type, RabbitMqEventConsumerHandler>();

        private ConcurrentDictionary<Type, IRequestClient> RequestClients { get; } =
            new ConcurrentDictionary<Type, IRequestClient>();

        private readonly IList<Type> _consumerTypes = new List<Type>();
        private readonly IList<Type> _eventTypes = new List<Type>();

        public RabbitMqTransportProvider(IEventLogger logger, IServiceResolver serviceResolver,
            IEventEncoder encoder, IConnection connection)
        {
            Encoder = encoder;
            Connection = connection;
            Logger = logger;
            ServiceResolver = serviceResolver;
        }

        public Result Start()
        {
            foreach (var eventType in _eventTypes)
            {
                var topologyAttribute = eventType.GetCustomAttributes(typeof(EventTopologyAttribute), false)
                    .FirstOrDefault() as EventTopologyAttribute;

                if (topologyAttribute == null)
                    throw new Exception($"Event type {eventType.Name} does not have EventTopologyAttribute");

                EventTopologies.TryAdd(eventType, topologyAttribute);
            }

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
            }

            return Result.Ok();
        }

        public Result Stop()
        {
            Consumers.Clear();
            return Result.Ok();
        }

        private RabbitMqRequestClient<T> GetRequestClient<T>() where T : IEvent
        {
            if (RequestClients.TryGetValue(typeof(T), out var client))
                return (RabbitMqRequestClient<T>)client;

            var requestClient = new RabbitMqRequestClient<T>(this);

            RequestClients.TryAdd(typeof(T), requestClient);

            return requestClient;
        }

        public Task<IResponse> RequestAsync<T>(T @event, IDictionary<string, object> headers = null,
            CancellationToken cancellationToken = default) where T : IEvent, ICorrelated
        {
            var topology = EventTopologies[@event.GetType()];
            
            if (headers == null)
                headers = new Dictionary<string, object>(topology.Headers);
            else
            {
                foreach (var header in topology.Headers.Where(x => !headers.ContainsKey(x.Key)))
                    headers.Add(header.Key, header.Value);
            }

            if (!headers.ContainsKey("x-message-id"))
                headers.Add("x-message-id", Guid.NewGuid().ToString());

            var requestClient = GetRequestClient<T>();

            return requestClient.RequestAsync<T>(@event, headers, cancellationToken);
        }

        public Task PublishAsync<T>(T @event, IDictionary<string, object> headers = null,
            CancellationToken cancellationToken = default) where T : IEvent, ICorrelated
        {
            using (var model = Connection.CreateModel())
            {
                var topology = EventTopologies[@event.GetType()];
                var body = Encoder.Encode(@event);

                var properties = model.CreateBasicProperties();
                
                foreach (var header in headers ?? new Dictionary<string, object>())
                    properties.Headers.Add(header.Key, header.Value);

                properties.CorrelationId = @event.CorrelationId.ToString();
                properties.ContentType = "application/json";
                properties.MessageId = Guid.NewGuid().ToString();

                model.BasicPublish(
                    topology.ExchangeName,
                    topology.RoutingKey,
                    properties,
                    body
                );
            }

            return Task.CompletedTask;
        }

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

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
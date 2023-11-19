using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Collections;
using Eventy.Events.Clients;
using Eventy.Events.Constants;
using Eventy.Events.Contracts;
using Eventy.Events.Models;
using Eventy.Events.States;
using Eventy.RabbitMQ.Contracts;
using Eventy.RabbitMQ.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Eventy.RabbitMQ.Clients
{
    public class RabbitMqRequestClient<TEvent> : IRequestClient<TEvent>
        where TEvent : IEvent
    {
        private readonly IModel _model;
        private readonly IEventTopology _topology;
        private readonly IRabbitMqTransportProvider _transportProvider;
        private readonly AsyncEventingBasicConsumer _consumer;

        public RabbitMqRequestClient(IRabbitMqTransportProvider transportProvider)
        {
            _transportProvider = transportProvider;
            _topology = _transportProvider.EventTopologies[typeof(TEvent)];
            _model = _transportProvider.Connection.CreateModel();
            var queueName = $"{_topology.QueueName}.callback";
            
            _model.BasicQos(0, 5, false);
            _model.ExchangeDeclare(_topology.ExchangeName, ExchangeType.Direct, true, false, null);
            _model.QueueDeclare(queueName, true, false, false, null);
            _model.QueueBind(queueName, _topology.ExchangeName, $"{_topology.RoutingKey}.callback", null);
            
            _consumer = new AsyncEventingBasicConsumer(_model);
            _consumer.Received += HandleCallbackAsync;

            _model.BasicConsume(queueName, false, _consumer);
        }

        public ConcurrentDictionary<string, IRequestState> PendingRequests { get; } =
            new ConcurrentDictionary<string, IRequestState>();

        public Task<IResponse> RequestAsync<T>(TEvent @event, IDictionary<string, object> headers, CancellationToken cancellationToken = default)
            where T : IEvent, ICorrelated
        {
            var headerCollection = new HeaderCollection(headers);
            
            var requestId = Guid.NewGuid().ToString();
            
            cancellationToken.Register(() => PendingRequests.TryRemove(requestId, out _));
            var state = new RequestState(requestId, cancellationToken);

            PendingRequests.TryAdd(requestId, state);

            var body = _transportProvider.Encoder.Encode(@event);

            var properties = _model.CreateBasicProperties();
            
            properties.CorrelationId = @event.CorrelationId ?? Guid.NewGuid().ToString();
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Expiration = "30000";
            
            headerCollection.AddHeader(HeaderConstants.RequestId, requestId);
            properties.Headers = headerCollection;
            
            _model.BasicPublish(_topology.ExchangeName, _topology.RoutingKey, properties, body);

            return state.TaskCompletionSource.Task;
        }

        private async Task HandleCallbackAsync(object sender, BasicDeliverEventArgs @event)
        {
            var headers =
                new HeaderCollection(@event.BasicProperties.Headers ?? new ConcurrentDictionary<string, object>());
            
            var body = @event.Body.ToArray();
            
            if (!headers.TryGetValue(HeaderConstants.RequestId, out var requestIdObj))
                requestIdObj = string.Empty;
            
            if (!PendingRequests.TryRemove((string)requestIdObj, out var state))
                return;
            
            var decodedEvent = _transportProvider.Encoder.Decode<IResponse>(body, typeof(RequestResponse));
            decodedEvent.Headers = headers;
            
            state.SetResponse(decodedEvent);
            
            _model.BasicAck(@event.DeliveryTag, false);
        }
        
        public void Dispose()
        {
            _transportProvider?.Dispose();
            _model?.Dispose();
        }
    }
}
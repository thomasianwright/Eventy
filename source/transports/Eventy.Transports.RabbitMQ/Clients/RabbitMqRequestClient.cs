using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Clients;
using Eventy.Events.Contracts;
using Eventy.Events.Models;
using Eventy.Events.States;
using Eventy.RabbitMQ.Contracts;
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

            _model.BasicQos(0, 5, false);
            _model.ExchangeDeclare(_topology.ExchangeName, ExchangeType.Direct, true, false, null);
            _model.QueueDeclare(_topology.CallbackQueueName, true, false, false, null);
            _model.QueueBind(_topology.CallbackQueueName, _topology.ExchangeName, _topology.CallbackQueueName, null);
            
            _consumer = new AsyncEventingBasicConsumer(_model);
            _consumer.Received += ConsumerOnReceived;

            _model.BasicConsume(_topology.CallbackQueueName, false, _consumer);
        }

        public ConcurrentDictionary<Guid, IRequestState> PendingRequests { get; } =
            new ConcurrentDictionary<Guid, IRequestState>();

        public Task<IResponse> RequestAsync<T>(TEvent @event, CancellationToken cancellationToken = default)
            where T : IEvent, ICorrelatedBy<Guid>
        {
            var state = new RequestState(@event.CorrelationId);

            PendingRequests.TryAdd(@event.CorrelationId, state);

            var body = _transportProvider.Encoder.Encode(@event);

            var properties = _model.CreateBasicProperties();
            properties.ReplyTo = _topology.CallbackQueueName;
            properties.CorrelationId = @event.CorrelationId.ToString();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            
            _model.BasicPublish(_topology.ExchangeName, _topology.RoutingKey, properties, body);

            return state.TaskCompletionSource.Task;
        }

        public void Dispose()
        {
            _transportProvider?.Dispose();
            _model?.Dispose();
        }

        private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
        {
            var headers = @event.BasicProperties.Headers ?? new ConcurrentDictionary<string, object>();
            var body = @event.Body.ToArray();

            if (string.IsNullOrEmpty(@event.BasicProperties.CorrelationId) ||
                !Guid.TryParse(@event.BasicProperties.CorrelationId, out var correlationGuid) ||
                !PendingRequests.TryRemove(correlationGuid, out var state))
            {
                _model.BasicReject(@event.DeliveryTag, false);
                return;
            }

            var decodedEvent = _transportProvider.Encoder.Decode<IResponse>(body, typeof(RequestResponse));

            state.SetResponse(decodedEvent);
            
            _model.BasicAck(@event.DeliveryTag, false);

            return;
        }
    }
}
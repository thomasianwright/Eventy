using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Consumers;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.Events.Encoders;
using Eventy.IoC.Services;
using Eventy.Logging.Services;
using Eventy.RabbitMQ.Contexts;
using Eventy.RabbitMQ.Contracts;
using Eventy.RabbitMQ.Extensions;
using FluentResults;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Eventy.RabbitMQ.Consumers
{
    public sealed class RabbitMqEventConsumerHandler : EventConsumerHandler<IRabbitMqTransportProvider>
    {
        private readonly MethodInfo _methodInfo;
        private readonly IModel _model;
        private CancellationToken _cancellationToken;
        private AsyncEventingBasicConsumer _consumer;

        public RabbitMqEventConsumerHandler(IEventLogger logger, IRabbitMqTransportProvider provider,
            IServiceResolver serviceResolver, Type consumerType, Type eventType, IEventEncoder encoder) : base(logger,
            provider, serviceResolver, consumerType, eventType)
        {
            Encoder = encoder;
            _model = Provider.Connection.CreateModel();
            _methodInfo = ConsumerType.GetMethod("ConsumeAsync");
            
            _model.ExchangeDeclare(Topology.ExchangeName, ExchangeType.Direct, true, false, null);
            _model.QueueDeclare(Topology.QueueName, true, false, false, null);
            _model.QueueBind(Topology.QueueName, Topology.ExchangeName, Topology.RoutingKey, null);

            _consumer = new AsyncEventingBasicConsumer(_model);
            _consumer.Received += ConsumerOnReceived;
            _model.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _model.BasicConsume(Topology.QueueName, false, _consumer);
        }

        private IEventEncoder Encoder { get; }

        public override Task StartAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var consumingChannel = ((AsyncEventingBasicConsumer)sender).Model;

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var headers = @event.BasicProperties.Headers ?? new Dictionary<string, object>();
                
                var body = @event.Body.ToArray();

                var decodedEvent = Encoder.Decode<IEvent>(body, EventType);

                var context = CreateEventContext(decodedEvent, headers) as RabbitMqEventContext ??
                              throw new InvalidOperationException("Invalid event context type");
                context.DeliveryTag = @event.DeliveryTag;
                context.ConsumingModel = consumingChannel;

                using (var scope = ServiceResolver.CreateScope())
                {
                    var consumer = scope.GetService(ConsumerType) as IConsumer ??
                                   throw new InvalidOperationException("Invalid consumer type");
                    consumer.Bus = Provider;
                    consumer.Context = context;

                    var result = await ((Task<Result>)_methodInfo.Invoke(consumer, new object[] { decodedEvent, cts.Token }));

                    if (result.IsSuccess)
                        context.Ack();
                    else
                        context.Nack();
                }
            } catch (Exception e)
            {
                Logger.LogError( $"Error while consuming event {EventType.Name}");
                
                _model.BasicNack(@event.DeliveryTag, false, true);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken = default)
        {
            _consumer.Received -= ConsumerOnReceived;
            _model.Dispose();

            return Task.CompletedTask;
        }

        protected override IEventContext CreateEventContext(IEvent @event, IDictionary<string, object> headers, Guid? messageId = null)
        {
            if (!messageId.HasValue)
            {
                messageId = Guid.NewGuid();
            }

            var requestIdExists = headers.GetHeader("x-request-id", out var requestId);
            var requestIdGuid = Guid.Empty;
            
            if (requestIdExists)
            {
                requestIdGuid = Guid.Parse(requestId);
            }
            
            return new RabbitMqEventContext(@event.CorrelationId, Topology, _model)
            {
                MessageId = messageId.Value,
                RequestId = requestIdGuid
            };
        }
    }
}
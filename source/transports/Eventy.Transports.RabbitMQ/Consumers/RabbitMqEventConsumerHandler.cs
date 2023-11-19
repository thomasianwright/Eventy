using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Collections;
using Eventy.Events.Constants;
using Eventy.Events.Consumers;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.Events.Encoders;
using Eventy.Events.Models;
using Eventy.IoC.Services;
using Eventy.Logging.Services;
using Eventy.RabbitMQ.Contexts;
using Eventy.RabbitMQ.Contracts;
using Eventy.RabbitMQ.Extensions;
using FluentResults;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Eventy.RabbitMQ.Consumers
{
    public sealed class RabbitMqEventConsumerHandler : EventConsumerHandler<IRabbitMqTransportProvider>, IDisposable
    {
        private readonly IModel _model;
        private AsyncEventingBasicConsumer _consumer;

        public RabbitMqEventConsumerHandler(IEventLogger logger, IRabbitMqTransportProvider provider,
            IServiceResolver serviceResolver, Type consumerType, Type eventType, IEventEncoder encoder) : base(logger,
            provider, serviceResolver, consumerType, eventType)
        {
            Encoder = encoder;
            _model = Provider.Connection.CreateModel();

            _model.ExchangeDeclare(Topology.ExchangeName, Topology.ExchangeType, true, false, null);
            _model.QueueDeclare(Topology.QueueName, true, false, false, null);
            _model.QueueBind(Topology.QueueName, Topology.ExchangeName, Topology.RoutingKey, null);

            _consumer = new AsyncEventingBasicConsumer(_model);
            _consumer.Received += ConsumerOnReceived;
            _model.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _model.BasicConsume(Topology.QueueName, false, _consumer);
            
            _consumeAsync = ConfigureConsumeAsync();
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
                var headers = new HeaderCollection(@event.BasicProperties.Headers ?? new Dictionary<string, object>());

                var body = @event.Body.ToArray();

                var decodedEvent = Encoder.Decode<IEvent>(body, EventType);
                
                if (!headers.TryGetValue(HeaderConstants.RequestId, out var requestIdObj))
                    requestIdObj = string.Empty;

                var context = new RabbitMqEventContext(@event.BasicProperties.CorrelationId,
                    @event.BasicProperties.MessageId, (string)requestIdObj, headers, Topology,
                    async (data, responseHeaders, isSuccess) =>
                    {
                        var json = JsonConvert.SerializeObject(data);
                        var messageId = Guid.NewGuid().ToString();
                        var correlationId = @event.BasicProperties.CorrelationId;
                        var resHeaders = new HeaderCollection(responseHeaders ?? new Dictionary<string, object>());
                        
                        var response = new RequestResponse
                        {
                            Body = json,
                            IsSuccess = isSuccess,
                            Type = data.GetType().Name,
                            Headers = responseHeaders,
                            CorrelationId = correlationId,
                            MessageId = messageId,
                        };

                        var responseBytes = Encoder.Encode(response);
                        var responseProperties = consumingChannel.CreateBasicProperties();
                        
                        responseProperties.CorrelationId = correlationId;
                        responseProperties.MessageId = messageId;
                        responseProperties.Persistent = true;
                        responseProperties.ContentType = "application/json";
                        responseProperties.Expiration = "30000";
                        
                        resHeaders.AddHeader(HeaderConstants.RequestId, (string)requestIdObj);

                        responseProperties.Headers = resHeaders;
                        
                        consumingChannel.BasicPublish(Topology.ExchangeName, $"{Topology.RoutingKey}.callback", responseProperties,
                            responseBytes);
                    });
                
                var result = await _consumeAsync(ConsumerType, decodedEvent, Provider, context);
                
                if (result.IsSuccess)
                    consumingChannel.BasicAck(@event.DeliveryTag, false);
                else
                    consumingChannel.BasicNack(@event.DeliveryTag, false, true);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while consuming event {EventType.Name}");

                _model.BasicNack(@event.DeliveryTag, false, true);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken = default)
        {
            _consumer.Received -= ConsumerOnReceived;
            _model.Dispose();

            return Task.CompletedTask;
        }

        private Func<object, object, IRabbitMqTransportProvider, IEventContext, Task<Result>> _consumeAsync;

        private Func<object, object, IRabbitMqTransportProvider, IEventContext, Task<Result>> ConfigureConsumeAsync()
        {
            var methodInfo = ConsumerType.GetMethod("ConsumeAsync") ??
                             throw new InvalidOperationException("Invalid consumer type");

            
            return async (handler, e, provider, ctx) =>
            {
                try
                {
                    using (var scope = ServiceResolver.CreateScope())
                    {
                        var consumer = scope.GetService(ConsumerType) as IConsumer ??
                                       throw new InvalidOperationException("Invalid consumer type");
                        consumer.Bus = Provider;
                        consumer.Context = ctx;

                        var result =
                            await ((Task<Result>)methodInfo.Invoke(consumer, new object[] { e, CancellationToken.None }));

                        return result;
                    }
                } catch (Exception ex)
                {
                    return Result.Fail(new Error(ex.Message));
                }
            };
        }

        public void Dispose()
        {
            _model?.Dispose();
        }
    }
}
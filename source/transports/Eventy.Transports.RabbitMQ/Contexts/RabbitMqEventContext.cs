using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.Events.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Eventy.RabbitMQ.Contexts
{
    public class RabbitMqEventContext : IEventContext
    {
        private readonly IModel _model;

        public RabbitMqEventContext(Guid? correlationId, IEventTopology topology, IModel model)
        {
            _model = model;
            CorrelationId = correlationId;
            Topology = topology;
        }
        
        internal IModel ConsumingModel { get; set; }

        internal ulong DeliveryTag { get; set; }
        public Guid? CorrelationId { get; }
        public IEventTopology Topology { get; }
        public Guid MessageId { get; set; }
        public Guid RequestId { get; set; }

        bool IsAcked { get; set; }
        bool IsNacked { get; set; }
        
        public Task RespondAsync<T>(T data, IDictionary<string, object> headers = null, bool isSuccess = true) where T : class
        {
            if (headers == null) 
                headers = new Dictionary<string, object>();
            var properties = _model.CreateBasicProperties();

            foreach (var header in headers)
            {
                properties.Headers.Add(header.Key, header.Value);
            }
            
            properties.Headers = properties.Headers ?? new Dictionary<string, object>();
            properties.CorrelationId = CorrelationId.ToString();
            properties.ContentType = "application/json";
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Headers.Add("x-request-id", RequestId.ToString());
            
            var response = new RequestResponse()
            {
                CorrelationId = CorrelationId ?? Guid.Empty,
                IsSuccess = isSuccess,
                Body = JsonConvert.SerializeObject(data)
            };
            
            Ack();
            
            _model.BasicPublish(
                Topology.ExchangeName,
                Topology.CallbackQueueName,
                properties,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response))
            );

            return Task.CompletedTask;
        }

        public void Ack()
        {
            if (_model.IsOpen && !IsAcked && !IsNacked)
            {
                IsAcked = true;
                _model.BasicAck(DeliveryTag, false);
            }
        }

        public void Nack(bool requeue = false)
        {
            if (_model.IsOpen && !IsNacked && !IsAcked)
            {
                IsNacked = true;
                _model.BasicNack(DeliveryTag, false, requeue);
            }
        }
    }
}
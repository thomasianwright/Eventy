using System;
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

        bool IsAcked { get; set; }
        bool IsNacked { get; set; }
        
        public Task RespondAsync<T>(T data, bool isSuccess = true) where T : class
        {
            var properties = _model.CreateBasicProperties();
            properties.CorrelationId = CorrelationId.ToString();
            properties.ContentType = "application/json";

            var response = new RequestResponse()
            {
                CorrelationId = CorrelationId ?? Guid.Empty,
                IsSuccess = isSuccess,
                Body = JsonConvert.SerializeObject(data)
            };
            
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
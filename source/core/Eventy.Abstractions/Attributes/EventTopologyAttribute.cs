using System;
using System.Collections.Generic;
using Eventy.Events.Contracts;

namespace Eventy.Events.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventTopologyAttribute : Attribute, IEventTopology
    {
        public EventTopologyAttribute(string queueName, string exchangeName = null, string routingKey = null,
            string callbackQueueName = null,  string exchangeType = null)
        {
            QueueName = queueName;
            ExchangeName = string.IsNullOrWhiteSpace(exchangeName) ? queueName : exchangeName;
            RoutingKey = string.IsNullOrWhiteSpace(routingKey) ? queueName : routingKey;
            CallbackQueueName = callbackQueueName ?? $"{queueName}.callback";
            
            ExchangeType = exchangeType ?? "direct";
        }

        public string QueueName { get; }
        public string ExchangeName { get; }
        public string RoutingKey { get; }
        public string CallbackQueueName { get; }
        public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

        public void AddHeader(string key, object value)
        {
            Headers.Add(key, value);
        }

        public string ExchangeType { get; set; }
    }
}
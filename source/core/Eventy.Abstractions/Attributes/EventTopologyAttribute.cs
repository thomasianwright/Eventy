using System;
using System.Collections.Generic;
using Eventy.Events.Contracts;

namespace Eventy.Events.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventTopologyAttribute : Attribute, IEventTopology
    {
        public EventTopologyAttribute(string queueName, string exchangeName = null, string routingKey = null,
            string callbackQueueName = null, bool requeue = false, bool autoAck = false, bool autoAckWhenConsumed = false)
        {
            QueueName = queueName;
            ExchangeName = string.IsNullOrWhiteSpace(exchangeName) ? queueName : exchangeName;
            RoutingKey = string.IsNullOrWhiteSpace(routingKey) ? queueName : routingKey;
            CallbackQueueName = $"{queueName}.callback";
            Requeue = requeue;
            
            AutoAck = autoAck;
            AutoAckWhenConsumed = autoAckWhenConsumed;
        }

        public string QueueName { get; }
        public string ExchangeName { get; }
        public string RoutingKey { get; }
        public string CallbackQueueName { get; }
        public bool Requeue { get; }
        public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();
        public void AddHeader(string key, object value)
        {
            Headers.Add(key, value);
        }

        public bool AutoAck { get; }
        public bool AutoAckWhenConsumed { get; }
    }
}
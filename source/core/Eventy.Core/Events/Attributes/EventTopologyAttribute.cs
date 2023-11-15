using System;
using Eventy.Events.Contracts;

namespace Eventy.Events.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventTopologyAttribute : Attribute, IEventTopology
    {
        public EventTopologyAttribute(string queueName, string exchangeName = null, string routingKey = null,
            string callbackQueueName = null, bool requeue = false)
        {
            QueueName = queueName;
            ExchangeName = string.IsNullOrWhiteSpace(exchangeName) ? queueName : exchangeName;
            RoutingKey = string.IsNullOrWhiteSpace(routingKey) ? queueName : routingKey;
            CallbackQueueName = $"{queueName}.callback";
            Requeue = requeue;
        }

        public string QueueName { get; }
        public string ExchangeName { get; }
        public string RoutingKey { get; }
        public string CallbackQueueName { get; }
        public bool Requeue { get; }
    }
}
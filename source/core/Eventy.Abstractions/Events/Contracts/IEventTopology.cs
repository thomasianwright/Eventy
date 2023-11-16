using System.Collections.Generic;

namespace Eventy.Events.Contracts
{
    public interface IEventTopology
    {
        string QueueName { get; }
        string ExchangeName { get; }
        string RoutingKey { get; }
        string CallbackQueueName { get; }
        bool Requeue { get; }
        
        IDictionary<string, object> Headers { get; }
        void AddHeader(string key, object value);
        
        bool AutoAck { get; }
        bool AutoAckWhenConsumed { get; }
    }
}
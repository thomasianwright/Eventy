using System.Collections.Generic;

namespace Eventy.Events.Contracts
{
    public interface IEventTopology
    {
        string QueueName { get; }
        string ExchangeName { get; }
        string RoutingKey { get; }
        
        IDictionary<string, object> Headers { get; }
        void AddHeader(string key, object value);
        
        string ExchangeType { get; }
    }
}
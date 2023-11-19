using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventy.Events.Contracts;

namespace Eventy.Events.Contexts
{
    public interface IEventContext
    {
        string CorrelationId { get; set; }
        string MessageId { get; set; }
        string RequestId { get; set; }
        
        IDictionary<string, object> Headers { get; }
        
        IEventTopology Topology { get; }
        Task RespondAsync<T>(T data, IDictionary<string, object> headers = null, bool isSuccess = true) where T : class;
    }
}
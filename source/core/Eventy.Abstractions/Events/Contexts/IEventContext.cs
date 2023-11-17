using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventy.Events.Contracts;

namespace Eventy.Events.Contexts
{
    public interface IEventContext
    {
        Guid? CorrelationId { get; }
        IEventTopology Topology { get; }
        Task RespondAsync<T>(T data, IDictionary<string, object> headers = null, bool isSuccess = true) where T : class;

        void Ack();
        void Nack(bool requeue = false);
    }
}
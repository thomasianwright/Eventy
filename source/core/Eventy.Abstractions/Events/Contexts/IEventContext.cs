using System;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Contracts;

namespace Eventy.Abstractions.Events.Contexts
{
    public interface IEventContext
    {
        Guid? CorrelationId { get; }
        IEventTopology Topology { get; }
        Task RespondAsync<T>(T data, bool isSuccess = true) where T : class;

        void Ack();
        void Nack(bool requeue = false);
    }
}
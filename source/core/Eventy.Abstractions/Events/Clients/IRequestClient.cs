using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Contracts;
using Eventy.Abstractions.Events.States;

namespace Eventy.Abstractions.Events.Clients
{
    public interface IRequestClient : IDisposable
    {
        ConcurrentDictionary<Guid, IRequestState> PendingRequests { get; }
    }

    public interface IRequestClient<in TEvent> : IRequestClient
        where TEvent : IEvent, ICorrelatedBy<Guid>
    {
        Task<IResponse> RequestAsync<T>(TEvent @event, CancellationToken cancellationToken = default)
            where T : IEvent, ICorrelatedBy<Guid>;
    }
}
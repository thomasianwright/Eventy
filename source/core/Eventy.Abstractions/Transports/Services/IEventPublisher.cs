using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Contracts;

namespace Eventy.Transports.Services
{
    public interface IEventPublisher
    {
        Task<IResponse> RequestAsync<T>(T @event, IDictionary<string, object> headers = null,
            CancellationToken cancellationToken = default) where T : IEvent, ICorrelated;

        Task PublishAsync<T>(T @event, IDictionary<string, object> headers = null, CancellationToken cancellationToken = default)
            where T : IEvent, ICorrelated;
    }
}
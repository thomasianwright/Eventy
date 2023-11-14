using System;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Contracts;

namespace Eventy.Abstractions.Transports.Services
{
    public interface IEventPublisher
    {
        Task<IResponse> RequestAsync<T>(T @event) where T : IEvent, ICorrelatedBy<Guid>;

        Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : IEvent, ICorrelatedBy<Guid>;

        Task SendAsync<T>(T @event) where T : IEvent, ICorrelatedBy<Guid>;
    }
}
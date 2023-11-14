using System.Threading;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Contexts;
using Eventy.Abstractions.Events.Contracts;
using Eventy.Abstractions.Transports.Services;
using FluentResults;

namespace Eventy.Abstractions.Events.Consumers
{
    public interface IConsumer
    {
        IBus Bus { get; set; }

        IEventContext Context { get; set; }
    }

    public interface IConsumer<in TEvent> : IConsumer
        where TEvent : IEvent
    {
        Task<Result> ConsumeAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
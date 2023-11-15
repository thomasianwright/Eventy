using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.Transports.Services;
using FluentResults;

namespace Eventy.Events.Consumers
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
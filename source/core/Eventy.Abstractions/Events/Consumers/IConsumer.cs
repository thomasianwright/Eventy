using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.Transports.Services;
using FluentResults;
using JetBrains.Annotations;

namespace Eventy.Events.Consumers
{
    public interface IConsumer
    {
        [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
        IBus Bus { get; set; }

        [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
        IEventContext Context { get; set; }
    }

    public interface IConsumer<in TEvent> : IConsumer
        where TEvent : IEvent
    {
        [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
        Task<Result> ConsumeAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
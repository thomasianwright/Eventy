using System;

namespace Eventy.Abstractions.Events.Contracts
{
    public interface IEvent : ICorrelatedBy<Guid>
    {
    }
}
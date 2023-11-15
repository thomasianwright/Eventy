using System;

namespace Eventy.Events.Contracts
{
    public interface IEvent : ICorrelatedBy<Guid>
    {
    }
}
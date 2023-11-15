using System;

namespace Eventy.Events.Contracts
{
    public interface IResponse : ICorrelatedBy<Guid>
    {
        string Body { get; }
        bool IsSuccess { get; }

        T Deserialize<T>();
    }
}
using System;
using System.Collections.Generic;

namespace Eventy.Events.Contracts
{
    public interface IResponse : ICorrelatedBy<Guid>
    {
        string Type { get; }
        string Body { get; }
        bool IsSuccess { get; }

        T Deserialize<T>();
        
        IDictionary<string, object> Headers { get; set; }
    }
}
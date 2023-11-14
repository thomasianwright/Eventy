using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Contracts;
using FluentResults;

namespace Eventy.Abstractions.Transports.Services
{
    public interface ITransportProvider : IBus, IDisposable
    {
        string Name { get; }

        ConcurrentDictionary<Type, IEventTopology> EventTopologies { get; }

        Result AddEventTypes(params Type[] eventTypes);
        Result AddConsumers(params Type[] consumers);

        Task<Result> Start();

        Result Stop();
    }
}
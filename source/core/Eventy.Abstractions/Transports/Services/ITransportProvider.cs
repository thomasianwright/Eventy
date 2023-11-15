using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Eventy.Events.Contracts;
using FluentResults;

namespace Eventy.Transports.Services
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
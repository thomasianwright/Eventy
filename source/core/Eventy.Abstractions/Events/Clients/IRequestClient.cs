﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Contracts;
using Eventy.Events.States;

namespace Eventy.Events.Clients
{
    public interface IRequestClient : IDisposable
    {
        ConcurrentDictionary<string, IRequestState> PendingRequests { get; }
    }

    public interface IRequestClient<in TEvent> : IRequestClient
        where TEvent : IEvent, ICorrelated
    {
        Task<IResponse> RequestAsync<T>(TEvent @event, IDictionary<string, object> headers,
            CancellationToken cancellationToken = default)
            where T : IEvent, ICorrelated;
    }
}
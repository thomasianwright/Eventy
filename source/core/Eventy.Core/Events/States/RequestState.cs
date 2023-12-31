﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Events.Contracts;

namespace Eventy.Events.States
{
    public class RequestState : IRequestState
    {
        public RequestState(string correlationId, CancellationToken cancellationToken)
        {
            CorrelationId = correlationId;
            TaskCompletionSource = new TaskCompletionSource<IResponse>(cancellationToken);
        }

        public RequestState(string correlationId)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cts.Token;

            CorrelationId = correlationId;
            TaskCompletionSource = new TaskCompletionSource<IResponse>(cancellationToken);
        }

        public string CorrelationId { get; set; }
        public TaskCompletionSource<IResponse> TaskCompletionSource { get; }

        public void SetResponse(IResponse response)
        {
            TaskCompletionSource.SetResult(response);
        }

        public void SetCanceled()
        {
            TaskCompletionSource.SetCanceled();
        }
    }
}
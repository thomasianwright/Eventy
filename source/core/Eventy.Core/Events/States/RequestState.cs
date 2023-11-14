using System;
using System.Threading;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Contracts;
using Eventy.Abstractions.Events.States;

namespace Eventy.Core.Events.States
{
    public class RequestState : IRequestState
    {
        public RequestState(Guid correlationId, CancellationToken cancellationToken)
        {
            CorrelationId = correlationId;
            TaskCompletionSource = new TaskCompletionSource<IResponse>(cancellationToken);
        }

        public RequestState(Guid correlationId)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cts.Token;

            CorrelationId = correlationId;
            TaskCompletionSource = new TaskCompletionSource<IResponse>(cancellationToken);
        }

        public Guid CorrelationId { get; set; }
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
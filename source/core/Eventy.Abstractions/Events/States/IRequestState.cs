using System;
using System.Threading.Tasks;
using Eventy.Abstractions.Events.Contracts;

namespace Eventy.Abstractions.Events.States
{
    public interface IRequestState : ICorrelatedBy<Guid>
    {
        TaskCompletionSource<IResponse> TaskCompletionSource { get; }

        void SetResponse(IResponse response);

        void SetCanceled();
    }
}
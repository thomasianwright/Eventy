using System;
using System.Threading.Tasks;
using Eventy.Events.Contracts;

namespace Eventy.Events.States
{
    public interface IRequestState : ICorrelatedBy<Guid>
    {
        TaskCompletionSource<IResponse> TaskCompletionSource { get; }

        void SetResponse(IResponse response);

        void SetCanceled();
    }
}
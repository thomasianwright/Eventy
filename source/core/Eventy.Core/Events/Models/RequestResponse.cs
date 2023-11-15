using System;
using Eventy.Events.Contracts;

namespace Eventy.Events.Models
{
    public class RequestResponse : IResponse
    {
        public Guid CorrelationId { get; set; }
        public string Body { get; set; }
        public bool IsSuccess { get; set; }

        public T Deserialize<T>()
        {
            throw new NotImplementedException();
        }
    }
}
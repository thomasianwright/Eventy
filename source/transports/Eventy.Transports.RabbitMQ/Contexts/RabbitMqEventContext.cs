using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eventy.Events.Contexts;
using Eventy.Events.Contracts;
using Eventy.Events.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Eventy.RabbitMQ.Contexts
{
    public class RabbitMqEventContext : IEventContext
    {
        private readonly Func<object, IDictionary<string, object>, bool, Task> _responseHandler;

        public RabbitMqEventContext(string correlationId, string messageId, string requestId,
            IDictionary<string, object> headers, IEventTopology topology, Func<object,
                IDictionary<string, object>, bool, Task> responseHandler)
        {
            _responseHandler = responseHandler;
            CorrelationId = correlationId;
            Topology = topology;
            MessageId = messageId;
            RequestId = requestId;
            Headers = headers;
        }

        public string CorrelationId { get; set; }
        public string MessageId { get; set; }
        public string RequestId { get; set; }
        public IDictionary<string, object> Headers { get; set; }
        public IEventTopology Topology { get; }


        public Task RespondAsync<T>(T data, IDictionary<string, object> headers = null, bool isSuccess = true)
            where T : class
        {
            return _responseHandler(data, headers, isSuccess);
        }
    }
}
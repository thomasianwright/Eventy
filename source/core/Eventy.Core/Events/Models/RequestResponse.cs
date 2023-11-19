using System;
using System.Collections.Generic;
using Eventy.Events.Contracts;
using Eventy.Events.Encoders;
using Newtonsoft.Json;

namespace Eventy.Events.Models
{
    public class RequestResponse : IResponse
    {
        public RequestResponse(IEventEncoder encoder, string type, string body, bool isSuccess, IDictionary<string, object> headers)
        {
            Headers = headers;
            Encoder = encoder;
            Type = type;
            Body = body;
            IsSuccess = isSuccess;
        }

        public RequestResponse()
        {
            
        }

        [JsonIgnore]
        public IEventEncoder Encoder { get; set; }

        public string CorrelationId { get; set; }
        public string MessageId { get; set; }
        
        public string Type { get; set; }
        public string Body { get; set; }
        public bool IsSuccess { get; set; }

        public T Deserialize<T>()
        {
            throw new NotImplementedException();
        }
        
        [JsonIgnore]
        public IDictionary<string, object> Headers { get; set; }
    }
}
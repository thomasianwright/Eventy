using Eventy.Abstractions.Events.Encoders;
using Eventy.Abstractions.Transports.Services;
using RabbitMQ.Client;

namespace Eventy.Transports.RabbitMQ.Contracts
{
    public interface IRabbitMqTransportProvider : ITransportProvider
    {
        IConnection Connection { get; }
        IEventEncoder Encoder { get; }
    }
}
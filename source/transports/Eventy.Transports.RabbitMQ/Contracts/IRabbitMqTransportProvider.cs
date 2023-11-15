using Eventy.Events.Encoders;
using Eventy.Transports.Services;
using RabbitMQ.Client;

namespace Eventy.RabbitMQ.Contracts
{
    public interface IRabbitMqTransportProvider : ITransportProvider
    {
        IConnection Connection { get; }
        IEventEncoder Encoder { get; }
    }
}
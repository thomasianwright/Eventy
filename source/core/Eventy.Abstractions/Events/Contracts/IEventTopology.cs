namespace Eventy.Events.Contracts
{
    public interface IEventTopology
    {
        string QueueName { get; }
        string ExchangeName { get; }
        string RoutingKey { get; }
        string CallbackQueueName { get; }
        bool Requeue { get; }
    }
}
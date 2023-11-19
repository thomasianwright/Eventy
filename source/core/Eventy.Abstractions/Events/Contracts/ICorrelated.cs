namespace Eventy.Events.Contracts
{
    public interface ICorrelated
    {
        string CorrelationId { get; set; }
    }
}
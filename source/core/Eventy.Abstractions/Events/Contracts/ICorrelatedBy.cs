namespace Eventy.Events.Contracts
{
    public interface ICorrelatedBy<T> where T : struct
    {
        T CorrelationId { get; set; }
    }
}
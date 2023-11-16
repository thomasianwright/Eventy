namespace Eventy.Logging.Services
{
    public interface IEventLogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogCritical(string message);
    }
}
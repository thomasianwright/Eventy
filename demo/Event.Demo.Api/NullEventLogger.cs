using Eventy.Logging.Services;

namespace Event.Demo.Api;

public class NullEventLogger : IEventLogger
{
    public IDictionary<string, object> Properties { get; }
    public void LogInformation(string message, IDictionary<string, object> properties = null)
    {
        
    }

    public void LogWarning(string message, IDictionary<string, object> properties = null)
    {
    }

    public void LogError(string message, IDictionary<string, object> properties = null)
    {
    }

    public void LogCritical(string message, IDictionary<string, object> properties = null)
    {
    }
}
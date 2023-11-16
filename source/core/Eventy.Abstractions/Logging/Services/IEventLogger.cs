using System.Collections.Generic;

namespace Eventy.Logging.Services
{
    public interface IEventLogger
    {
        IDictionary<string, object> Properties { get; }
        
        void LogInformation(string message, IDictionary<string, object> properties = null);
        void LogWarning(string message, IDictionary<string, object> properties = null);
        void LogError(string message, IDictionary<string, object> properties = null);
        void LogCritical(string message, IDictionary<string, object> properties = null);
    }
}
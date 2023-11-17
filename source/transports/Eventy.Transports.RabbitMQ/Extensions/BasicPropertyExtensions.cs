using System.Collections.Generic;
using RabbitMQ.Client;

namespace Eventy.RabbitMQ.Extensions
{
    public static class BasicPropertyExtensions
    {
        public static void AddHeader(this IDictionary<string, object> properties, string key, object value)
        {
            if (properties == null)
                properties = new Dictionary<string, object>();
            
            properties[key] = value;
        }
        
        public static bool GetHeader(this IDictionary<string, object> properties, string key, out string val)
        {
            if (properties == null)
            {
                val = null;
                return false;
            }
            
            if (!properties.TryGetValue(key, out var objVal))
            {
                val = null;
                return false;
            }
            
            var utf8Bytes = (byte[]) objVal;
            val = System.Text.Encoding.UTF8.GetString(utf8Bytes);
            
            return true;
        }
    }
}
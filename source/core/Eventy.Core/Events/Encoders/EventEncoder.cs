using System;
using System.Text;
using Newtonsoft.Json;

namespace Eventy.Events.Encoders
{
    public class EventEncoder : IEventEncoder
    {
        public byte[] Encode<T>(T @event)
        {
            var json = JsonConvert.SerializeObject(@event);

            return Encoding.UTF8.GetBytes(json);
        }

        public T Decode<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public T Decode<T>(byte[] bytes, Type type)
        {
            var json = Encoding.UTF8.GetString(bytes);

            return (T)JsonConvert.DeserializeObject(json, type);
        }
    }
}
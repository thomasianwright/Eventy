using System;

namespace Eventy.Events.Encoders
{
    public interface IEventEncoder
    {
        byte[] Encode<T>(T @event);

        T Decode<T>(byte[] bytes);
        T Decode<T>(byte[] bytes, Type type);
    }
}
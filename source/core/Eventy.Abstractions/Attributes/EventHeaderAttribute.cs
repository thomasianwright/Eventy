namespace Eventy.Events.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class EventHeaderAttribute : System.Attribute
    {
        public string Key { get; }
        public object Value { get; }

        public EventHeaderAttribute(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
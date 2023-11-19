using System.Collections.Generic;
using System.Linq;

namespace Eventy.Collections
{
    public class HeaderCollection : Dictionary<string, object>
    {
        public HeaderCollection()
        {
        }

        public HeaderCollection(IDictionary<string, object> dictionary) : base(dictionary)
        {
            foreach (var kv in dictionary)
            {
                if (kv.Value is byte[] bytes) 
                    this[kv.Key] = System.Text.Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
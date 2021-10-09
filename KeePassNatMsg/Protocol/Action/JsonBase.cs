using Newtonsoft.Json.Linq;

namespace KeePassNatMsg.Protocol.Action
{
    public class JsonBase : JObject
    {
        public JsonBase() : base()
        {
        }

        public JsonBase(JObject obj) : base(obj)
        {
        }

        public void AddBytes(string key, byte[] data)
        {
            Add(key, System.Convert.ToBase64String(data));
        }

        public string GetString(string key)
        {
            var value = this[key] as JValue;
            return value == null ? null : value.Value as string;
        }

        public byte[] GetBytes(string key)
        {
            return System.Convert.FromBase64String(GetString(key));
        }
    }
}

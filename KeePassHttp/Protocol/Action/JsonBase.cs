using Newtonsoft.Json.Linq;

namespace KeePassHttp.Protocol.Action
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
            Add(key, new JValue(System.Convert.ToBase64String(data)));
        }

        public string GetString(string key) => (this[key] as JValue)?.Value as string;

        public byte[] GetBytes(string key) => System.Convert.FromBase64String(GetString(key));
    }
}

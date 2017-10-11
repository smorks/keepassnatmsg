
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KeePassHttp.Protocol.Action
{
    public class Request : JsonBase
    {
        public Request(JObject obj) : base(obj)
        {
        }

        public static Request ReadFromStream(System.IO.Stream s)
        {
            var reader = new JsonTextReader(new System.IO.StreamReader(s));
            return new Request((JObject)ReadFrom(reader));
        }

        public string Action => GetString("action");

        public string Message => GetString("message");

        public string Nonce => GetString("nonce");

        public Response GetResponse() => new Response(Action);

    }
}

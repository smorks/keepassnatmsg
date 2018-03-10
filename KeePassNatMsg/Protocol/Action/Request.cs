
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace KeePassNatMsg.Protocol.Action
{
    public class Request : JsonBase
    {
        private JsonBase _msg;

        public Request(JObject obj) : base(obj)
        {
        }

        public static Request ReadFromStream(System.IO.Stream s)
        {
            var reader = new JsonTextReader(new System.IO.StreamReader(s));
            return new Request((JObject)ReadFrom(reader));
        }

        public static Request FromString(string s)
        {
            var rdr = new JsonTextReader(new System.IO.StringReader(s));
            return new Request((JObject)ReadFrom(rdr));
        }

        public string ClientId => GetString("clientID");

        public string Action => GetString("action");

        public string Nonce => GetString("nonce");

        public byte[] NonceBytes => GetBytes("nonce");

        public JsonBase Message => _msg;

        public Response GetResponse() => new Response(this);

        public Response GetResponse(bool createMessage) => new Response(this, createMessage);

        public bool TryDecrypt()
        {
            try
            {
                _msg = KeePassNatMsgExt.CryptoHelper.DecryptMessage(ClientId, GetBytes("message"), GetBytes("nonce"));
                return true;
            }
            catch (Exception)
            {
                _msg = null;
            }
            return false;
        }
    }
}

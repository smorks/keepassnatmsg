using KeePassHttp.Protocol.Crypto;
using Newtonsoft.Json.Linq;

namespace KeePassHttp.Protocol.Action
{
    public class Response : JsonBase
    {
        private JsonBase _msg;

        public Response(string action)
        {
            Add("action", new JValue(action));
            AddBytes("nonce", Helper.GenerateNonce());
            CreateMessage();
        }

        public byte[] Nonce => GetBytes("nonce");

        public JsonBase Message => _msg;

        public string GetEncryptedResponse()
        {
            if (_msg != null)
            {
                AddBytes("message", KeePassHttpExt.CryptoHelper.EncryptMessage(_msg.ToString(), Nonce));
            }
            return ToString();
        }

        private void CreateMessage()
        {
            _msg = new JsonBase
            {
                {"hash", KeePassHttpExt.ExtInstance.GetDbHash()},
                {"version", GetVersion()},
                {"success", "true"},
                {"nonce", Nonce}
            };
        }

        private string GetVersion() => typeof(Response).Assembly.GetName().Version.ToString();
    }
}

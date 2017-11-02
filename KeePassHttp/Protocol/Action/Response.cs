using KeePassHttp.Protocol.Crypto;
using Newtonsoft.Json.Linq;

namespace KeePassHttp.Protocol.Action
{
    public class Response : JsonBase
    {
        private JsonBase _msg;

        public Response(string action)
        {
            Init(action, true);
        }

        public Response(string action, bool createMessage)
        {
            Init(action, createMessage);
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

        private void Init(string action, bool createMessage)
        {
            Add("action", new JValue(action));
            AddBytes("nonce", Helper.GenerateNonce());
            if (createMessage) CreateMessage();
        }

        private void CreateMessage()
        {
            _msg = new JsonBase
            {
                {"hash", KeePassHttpExt.ExtInstance.GetDbHash()},
                {"version", KeePassHttpExt.GetVersion()},
                {"success", "true"},
                {"nonce", Nonce}
            };
        }
    }
}

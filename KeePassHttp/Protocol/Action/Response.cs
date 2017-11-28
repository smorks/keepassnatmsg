using KeePassHttp.Protocol.Crypto;
using Newtonsoft.Json.Linq;

namespace KeePassHttp.Protocol.Action
{
    public class Response : JsonBase
    {
        private JsonBase _msg;

        public Response(Request req, bool createMessage)
        {
            Init(req, createMessage);
        }

        public Response(Request req)
        {
            Init(req, true);
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

        private void Init(Request req, bool createMessage)
        {
            Add("action", new JValue(req.Action));
            AddBytes("nonce", Helper.GenerateNonce(req.NonceBytes));
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

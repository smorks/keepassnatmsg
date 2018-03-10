using KeePassNatMsg.Protocol.Crypto;
using Newtonsoft.Json.Linq;

namespace KeePassNatMsg.Protocol.Action
{
    public class Response : JsonBase
    {
        private JsonBase _msg;
        private string _clientId;

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
                AddBytes("message", KeePassNatMsgExt.CryptoHelper.EncryptMessage(_clientId, _msg.ToString(), Nonce));
            }
            return ToString();
        }

        private void Init(Request req, bool createMessage)
        {
            Add("action", new JValue(req.Action));
            AddBytes("nonce", Helper.GenerateNonce(req.NonceBytes));
            if (createMessage) CreateMessage();
            _clientId = req.ClientId;
        }

        private void CreateMessage()
        {
            _msg = new JsonBase
            {
                {"hash", KeePassNatMsgExt.ExtInstance.GetDbHash()},
                {"version", KeePassNatMsgExt.GetVersion()},
                {"success", "true"},
                {"nonce", Nonce}
            };
        }
    }
}

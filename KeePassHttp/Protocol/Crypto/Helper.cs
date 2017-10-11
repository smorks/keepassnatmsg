
using KeePassHttp.Protocol.Action;
using Newtonsoft.Json.Linq;
using System.Text;

namespace KeePassHttp.Protocol.Crypto
{
    public sealed class Helper
    {
        private KeyPair _pair;

        public byte[] ClientPublicKey { get; set; }

        public JsonBase DecryptMessage(Request req)
        {
            var msg = System.Convert.FromBase64String(req.Message);
            var nonce = System.Convert.FromBase64String(req.Nonce);
            var data = TweetNaCl.CryptoBoxOpen(msg, nonce, ClientPublicKey, _pair.PrivateKey);
            return new JsonBase(JObject.Parse(Encoding.UTF8.GetString(data)));
        }

        public void EncryptMessage(Response resp, string msg)
        {
            var data = TweetNaCl.CryptoBox(Encoding.UTF8.GetBytes(msg), resp.Nonce, ClientPublicKey, _pair.PrivateKey);
            resp.SetMessage(data);
        }

        public KeyPair GenerateKeyPair()
        {
            _pair = new KeyPair();
            return _pair;
        }

        public void SetKeyPair(KeyPair pair)
        {
            _pair = pair;
        }

        public static byte[] GenerateNonce()
        {
            var nonce = new byte[TweetNaCl.BoxNonceBytes];
            TweetNaCl.RandomBytes(nonce);
            return nonce;
        }
    }
}

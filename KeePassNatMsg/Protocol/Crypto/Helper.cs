
using KeePassNatMsg.Protocol.Action;
using Newtonsoft.Json.Linq;
using System.Text;

namespace KeePassNatMsg.Protocol.Crypto
{
    public sealed class Helper
    {
        private KeyPair _pair;
        private UTF8Encoding _utf8 = new UTF8Encoding(false);

        public byte[] ClientPublicKey { get; set; }

        public JsonBase DecryptMessage(byte[] message, byte[] nonce)
        {
            if (_pair == null) return null;
            var data = TweetNaCl.CryptoBoxOpen(message, nonce, ClientPublicKey, _pair.PrivateKey);
            return new JsonBase(JObject.Parse(_utf8.GetString(data)));
        }

        public byte[] EncryptMessage(string msg, byte[] nonce)
        {
            return TweetNaCl.CryptoBox(_utf8.GetBytes(msg), nonce, ClientPublicKey, _pair.PrivateKey);
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

        public static byte[] GenerateNonce(byte[] nonce) => TweetNaCl.Increment(nonce);
    }
}

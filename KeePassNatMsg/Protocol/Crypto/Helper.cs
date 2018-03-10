
using KeePassNatMsg.Protocol.Action;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace KeePassNatMsg.Protocol.Crypto
{
    public sealed class Helper
    {
        private UTF8Encoding _utf8 = new UTF8Encoding(false);
        private Dictionary<string, KeyPair> _clientKeys;

        public Helper()
        {
            _clientKeys = new Dictionary<string, KeyPair>();
        }

        public JsonBase DecryptMessage(string clientId, byte[] message, byte[] nonce)
        {
            if (_clientKeys.ContainsKey(clientId))
            {
                var pair = _clientKeys[clientId];
                var data = TweetNaCl.CryptoBoxOpen(message, nonce, pair.PublicKey, pair.PrivateKey);
                return new JsonBase(JObject.Parse(_utf8.GetString(data)));
            }
            return null;
        }

        public byte[] EncryptMessage(string clientId, string msg, byte[] nonce)
        {
            if (_clientKeys.ContainsKey(clientId))
            {
                var pair = _clientKeys[clientId];
                return TweetNaCl.CryptoBox(_utf8.GetBytes(msg), nonce, pair.PublicKey, pair.PrivateKey);
            }
            return null;
        }

        public byte[] GenerateKeyPair(string clientId, byte[] clientPublicKey)
        {
            var pair = new KeyPair();

            if (_clientKeys.ContainsKey(clientId))
            {
                _clientKeys.Remove(clientId);
            }

            _clientKeys.Add(clientId, new KeyPair(pair.PrivateKey, clientPublicKey));

            return pair.PublicKey;
        }

        public byte[] ClientPublicKey(string clientId)
        {
            if (_clientKeys.ContainsKey(clientId))
            {
                return _clientKeys[clientId].PublicKey;
            }
            return null;
        }

        public static byte[] GenerateNonce(byte[] nonce) => TweetNaCl.Increment(nonce);
    }
}

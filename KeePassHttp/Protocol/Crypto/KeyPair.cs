using System;

namespace KeePassHttp.Protocol.Crypto
{
    public sealed class KeyPair
    {
        public byte[] PrivateKey { get; private set; }
        public byte[] PublicKey { get; private set; }

        public KeyPair()
        {
            PrivateKey = new byte[TweetNaCl.BoxSecretKeyBytes];
            PublicKey = TweetNaCl.CryptoBoxKeypair(PrivateKey);
        }

        public KeyPair(byte[] privateKey, byte[] publicKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        public string ToBase64()
        {
            var ms = new System.IO.MemoryStream();
            ms.Write(PrivateKey, 0, PrivateKey.Length);
            ms.Write(PublicKey, 0, PublicKey.Length);
            return Convert.ToBase64String(ms.ToArray());
        }

        public static KeyPair FromBase64(string s)
        {
            var data = Convert.FromBase64String(s);
            var sk = new byte[TweetNaCl.BoxSecretKeyBytes];
            var pk = new byte[TweetNaCl.BoxPublicKeyBytes];
            Array.Copy(data, sk, sk.Length);
            Array.Copy(data, sk.Length, pk, 0, pk.Length);
            return new KeyPair(sk, pk);
        }
    }
}

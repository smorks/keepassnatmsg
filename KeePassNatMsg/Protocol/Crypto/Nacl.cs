/*
 *This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace KeePassNatMsg.Protocol.Crypto
{
    public class TweetNaCl
    {
        #region Public-key cryptography - Implementation of curve25519-xsalsa20-poly1305

        /// <summary>
        /// crypto_scalarmult_curve25519
        /// </summary>
        public static readonly Int32 ScalarmultBytes = 32;

        /// <summary>
        /// crypto_scalarmult_curve25519
        /// </summary>
        public static readonly Int32 ScalarBytes = 32;
        
        /// <summary>
        /// crypto_box_beforenm computed shared key size 
        /// </summary>
        public static readonly Int32 BoxBeforenmBytes = 32;
        public static readonly Int32 BoxPublicKeyBytes = 32;
        public static readonly Int32 BoxSecretKeyBytes = 32;
        public static readonly Int32 BoxNonceBytes = 24;
        public static readonly Int32 BoxZeroBytes = 32;
        public static readonly Int32 BoxBoxZeroBytes = 16;

        public static readonly Int32 SecretBoxNonceBytes = 24;
        public static readonly Int32 SecretBoxKeyBytes = 32;
        
        /// <summary>
        /// SHA-512 hash bytes
        /// </summary>
        public static readonly Int32 HashBytes = 64;
        
        public static readonly Int32 SignPublicKeyBytes = 32;
        public static readonly Int32 SignSecretKeyBytes = 64;

        public static readonly Int32 SignBytes = 64;


        public class InvalidSignatureException : CryptographicException { }
        public class InvalidCipherTextException : CryptographicException { }
        public class InvalidEncryptionKeypair : CryptographicException { }

        /// <summary>
        /// Scalar multiplication is a curve25519 implementation.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p"></param>
        /// <returns>the resulting group element q of length SCALARMULT_BYTES.</returns>
        public static Byte[] CryptoScalarmult(Byte[] n, Byte[] p)
        {
            Byte[] q = new Byte[ScalarmultBytes];
            Byte[] z = new Byte[32];
            Int64[] x = new Int64[80];
            Int64[] a = new Int64[GF_LEN],
                b = new Int64[GF_LEN],
                c = new Int64[GF_LEN],
                d = new Int64[GF_LEN],
                e = new Int64[GF_LEN],
                f = new Int64[GF_LEN]
            ;

            Int32 r = 0;

            for (var i = 0; i < 31; ++i)
            {
                z[i] = n[i];
            }

            z[31] = (Byte)((n[31] & 127) | 64);
            z[0] &= 248;

            Unpack25519(x, p);

            for (var i = 0; i < 16; ++i)
            {
                b[i] = x[i];
                d[i] = a[i] = c[i] = 0;
            }

            a[0] = d[0] = 1;

            for (var i = 254; i >= 0; --i)
            {
                r = ((0xff & z[i >> 3]) >> (i & 7)) & 1;
                Sel25519(a, b, r);
                Sel25519(c, d, r);
                A(e, a, c);
                Z(a, a, c);
                A(c, b, d);
                Z(b, b, d);
                S(d, e);
                S(f, a);
                M(a, 0, c, 0, a, 0);
                M(c, 0, b, 0, e, 0);
                A(e, a, c);
                Z(a, a, c);
                S(b, a);
                Z(c, d, f);
                M(a, 0, c, 0, _121665, 0);
                A(a, a, d);
                M(c, 0, c, 0, a, 0);
                M(a, 0, d, 0, f, 0);
                M(d, 0, b, 0, x, 0);
                S(b, e);
                Sel25519(a, b, r);
                Sel25519(c, d, r);
            }
            for (var i = 0; i < 16; ++i)
            {
                x[i + 16] = a[i];
                x[i + 32] = c[i];
                x[i + 48] = b[i];
                x[i + 64] = d[i];
            }

            Inv25519(x, 32, x, 32);

            M(x, 16, x, 16, x, 32);

            Pack25519(q, x, 16);

            return q;
        }

        /// <summary>
        /// The crypto_box_keypair function randomly generates a secret key and a corresponding public key. 
        /// It guarantees that sk has crypto_box_SECRETKEYBYTES bytes and that pk has crypto_box_PUBLICKEYBYTES bytes. 
        /// </summary>
        /// <param name="secretKey">generated secret key</param>
        /// <returns>generated public key</returns>
        public static Byte[] CryptoBoxKeypair(Byte[] secretKey)
        {
            RandomBytes(secretKey);
            return CryptoScalarmultBase(secretKey);
        }

        /// <summary>
        /// The intermediate data computed by crypto_box_beforenm
        /// Applications that send several messages to the same receiver can gain speed by splitting CryptoBox into two steps, 
        /// <b>CryptoBoxBeforenm</b> and CryptoBoxAfternm. 
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="secretKey"></param>
        /// <returns>
        ///     shared key to be used with afternm
        /// </returns>
        public static Byte[] CryptoBoxBeforenm(Byte[] publicKey, Byte[] secretKey)
        {
            Byte[] s = CryptoScalarmult(secretKey, publicKey);
            return CryptoCoreHSalsa20(_0, s, Sigma);
        }

        /// <summary>
        /// Applications that send several messages to the same receiver can gain speed by splitting CryptoBox into two steps, 
        /// CryptoBoxBeforenm and <b>CryptoBoxAfternm</b>. 
        /// </summary>
        /// <param name="cipheredMessage"></param>
        /// <param name="paddedMessage"></param>
        /// <param name="nonce"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Byte[] CryptoBoxAfternm(Byte[] message, Byte[] nonce, Byte[] k)
        {
            return CryptoSecretBox(message, nonce, k);
        }

        /// <summary>
        /// Applications that receive several messages from the same 
        /// sender can gain speed by splitting CryptoBoxOpen into two steps, CryptoBoxBeforenm and <b>CryptoBoxOpenAfternm</b>. 
        /// </summary>
        /// <param name="paddedMessage"></param>
        /// <param name="cipheredMessage"></param>
        /// <param name="nonce"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Byte[] CryptoBoxOpenAfternm(Byte[] cipheredMessage, Byte[] nonce, Byte[] k)
        {
            return CryptoSecretBoxOpen(cipheredMessage, nonce, k);
        }

        /// <summary>
        /// The crypto_box function encrypts and authenticates a message m, remember that all 
        /// messages are 0-padded versions of messages with BOX_ZEROBYTES.
        /// </summary>
        /// <param name="cipheredMessage"></param>
        /// <param name="paddedMessage"></param>
        /// <param name="nonce"></param>
        /// <param name="publicKey"></param>
        /// <param name="secretKey"></param>
        /// <returns>
        ///     0 for success or -1 for failure
        /// </returns>
        public static Byte[] CryptoBox(Byte[] message, Byte[] nonce, Byte[] publicKey, Byte[] secretKey)
        {
            Byte[] k = CryptoBoxBeforenm(publicKey, secretKey);
            return CryptoBoxAfternm(message, nonce, k);
        }

        /// <summary>
        /// The crypto_box_open function verifies and decrypts a ciphertext
        /// </summary>
        /// <param name="paddedMessage"></param>
        /// <param name="cipheredMessage"></param>
        /// <param name="d"></param>
        /// <param name="nonce"></param>
        /// <param name="publicKey"></param>
        /// <param name="secretKey"></param>
        /// <returns>
        ///     0 for success or -1 for failure
        /// </returns>
        public static Byte[] CryptoBoxOpen(Byte[] cipheredMessage, Byte[] nonce, Byte[] publicKey, Byte[] secretKey)
        {
            Byte[] k = CryptoBoxBeforenm(publicKey, secretKey);
            return CryptoBoxOpenAfternm(cipheredMessage, nonce, k);
        }

        /// <summary>
        /// Randomly generates a secret key and a corresponding public key NaCl-compatible Ed25519 https://ed25519.cr.yp.to/
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static Byte[] CryptoSignKeypair(Byte[] secretKey)
        {
            Byte[] publicKey = new Byte[SignPublicKeyBytes];
            Byte[] d = new Byte[64];
            Int64[][] /*gf*/ p = new Int64[4][] { new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN] };

            RandomBytes(secretKey);

            if (CryptoHash(d, secretKey, 32) != 0)
            {
                throw new InvalidSignatureException();
            }

            d[0] &= 248;
            d[31] &= 127;
            d[31] |= 64;

            Scalarbase(p, d, 0);
            Pack(publicKey, p);

            for (var i = 0; i < 32; ++i)
            {
                secretKey[32 + i] = publicKey[i];
            }

            return publicKey;
        }
        
        /// <summary>
        /// The crypto_sign function signs a message m using the signer's secret key secretKey.
        /// </summary>
        /// <param name="signedMessage"></param>
        /// <param name="message"></param>
        /// <param name="n"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static Byte[] CryptoSign(Byte[] message, Byte[] secretKey)
        {
            Byte[] signedMessage = new Byte[SignBytes + message.Length];

            Byte[] d = new Byte[64];
            Byte[] h = new Byte[64];
            Byte[] r = new Byte[64];
            Int64[] x = new Int64[64];
            Int64[][] /*gf*/ p/*[4]*/ = new Int64[4][] { new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN] };

            CryptoHash(d, secretKey, 32);

            d[0] &= 248;
            d[31] &= 127;
            d[31] |= 64;

            for (var i = 0; i < message.Length; ++i)
            {
                signedMessage[64 + i] = message[i];
            }

            for (var i = 0; i < 32; ++i)
            {
                signedMessage[32 + i] = d[32 + i];
            }

            Byte[] smd = new Byte[signedMessage.Length];
            Array.Copy(signedMessage, 32, smd, 0, signedMessage.Length - 32);
            CryptoHash(r, smd, message.Length + 64);

            Reduce(r);
            Scalarbase(p, r, 0);
            Pack(signedMessage, p);

            for (int i = 0; i < 32; ++i)
            {
                signedMessage[i + 32] = secretKey[i + 32];
            }

            CryptoHash(h, signedMessage, message.Length + 64);
            Reduce(h);

            for (var i = 0; i < 64; ++i)
            {
                x[i] = 0;
            }

            for (var i = 0; i < 32; ++i)
            {
                x[i] = 0xff & r[i];
            }

            for (var i = 0; i < 32; ++i)
            {
                for (int j = 0; j < 32; ++j)
                {
                    x[i + j] += (0xff & h[i]) * (0xff & d[j]);
                }
            }

            ModL(signedMessage, 32, x);

            return signedMessage;
        }

        /// <summary>
        /// The CryptoSignOpen function verifies the signature
        /// </summary>
        /// <param name="message">returned clear message</param>
        /// <param name="signedMessage">signed message</param>
        /// <param name="publicKey">public key</param>
        /// <returns></returns>
        public static Byte[] CryptoSignOpen(Byte[] signedMessage, Byte[] publicKey)
        {
            Byte[] message = new Byte[signedMessage.Length - 64];

            Byte[] t = new Byte[32];
            Byte[] h = new Byte[64];
            Int64[][] /*gf*/ p = new Int64[4][] { new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN] };
            Int64[][] /*gf*/ q = new Int64[4][] { new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN] };
            Int32 messageSize = signedMessage.Length;

            Byte[] tsm = new Byte[signedMessage.Length];

            if (signedMessage.Length < 64)
            {
                throw new InvalidSignatureException();
            }

            if (Unpackneg(q, publicKey) != 0)
            {
                throw new InvalidSignatureException();
            }

            for (var i = 0; i < signedMessage.Length; ++i)
            {
                tsm[i] = signedMessage[i];
            }

            for (var i = 0; i < 32; ++i)
            {
                tsm[i + 32] = publicKey[i];
            }

            CryptoHash(h, tsm, signedMessage.Length);
            Reduce(h);
            Scalarmult(p, q, h, 0);

            Scalarbase(q, signedMessage, 32);
            Add(p, q);
            Pack(t, p);

            if (CryptoVerify32(signedMessage, t) != 0)
            {
                for (var i = 0; i < messageSize; ++i)
                {
                    tsm[i] = 0;
                }

                throw new InvalidSignatureException();
            }

            for (var i = 0; i < signedMessage.Length - 64; ++i)
            {
                message[i] = signedMessage[i + 64];
            }

            return message;
        }


        #endregion

        #region private methods

        #region Curve25519

        private static Byte[] Core(Byte[] pout, Byte[] pin, Byte[] k, Byte[] c, Boolean hsalsa)
        {
            UInt32[] w = new UInt32[16];
            UInt32[] x = new UInt32[16];
            UInt32[] y = new UInt32[16];
            UInt32[] t = new UInt32[4];

            Int32 i, j, m;


            for (i = 0; i < 4; i++)
            {
                x[5 * i] = Ld32(c, 4 * i);
                x[1 + i] = Ld32(k, 4 * i);
                x[6 + i] = Ld32(pin, 4 * i);
                x[11 + i] = Ld32(k, 16 + 4 * i);
            }

            for (i = 0; i < 16; ++i)
            {
                y[i] = x[i];
            }

            for (i = 0; i < 20; ++i)
            {
                for (j = 0; j < 4; ++j)
                {
                    for (m = 0; m < 4; ++m)
                    {
                        t[m] = x[(5 * j + 4 * m) % 16];
                    }

                    t[1] ^= L32(t[0] + t[3], 7);
                    t[2] ^= L32(t[1] + t[0], 9);
                    t[3] ^= L32(t[2] + t[1], 13);
                    t[0] ^= L32(t[3] + t[2], 18);

                    for (m = 0; m < 4; ++m)
                    {
                        w[4 * j + (j + m) % 4] = t[m];
                    }
                }

                for (m = 0; m < 16; ++m)
                {
                    x[m] = w[m];
                }
            }

            if (hsalsa)
            {
                for (i = 0; i < 16; ++i)
                {
                    x[i] += y[i];
                }

                for (i = 0; i < 4; ++i)
                {
                    x[5 * i] -= Ld32(c, 4 * i);
                    x[6 + i] -= Ld32(pin, 4 * i);
                }

                for (i = 0; i < 4; ++i)
                {
                    St32(pout, x[5 * i], 4 * i);
                    St32(pout, x[6 + i], 16 + 4 * i);
                }
            }
            else
            {
                for (i = 0; i < 16; ++i)
                {
                    St32(pout, x[i] + y[i], 4 * i);
                }
            }

            return pout;
        }

        private static Byte[] CryptoCoreHSalsa20(Byte[] pin, Byte[] k, Byte[] c)
        {
            Byte[] pout = new Byte[BoxBeforenmBytes];
            return Core(pout, pin, k, c, true);
        }
        
        /// <summary>
        /// The crypto_scalarmult_base function computes the scalar product of a standard group element and an integer n.
        /// </summary>
        /// <param name="n"></param>
        /// <returns>It returns the resulting group element q of length SCALARMULT_BYTES</returns>
        private static Byte[] CryptoScalarmultBase(Byte[] n)
        {
            return CryptoScalarmult(n, _9);
        }
        
        private static void Set25519(Int64[] /*gf*/ r, Int64[] /*gf*/ a)
        {
            for (var i = 0; i < 16; ++i)
            {
                r[i] = a[i];
            }
        }

        private static void Car25519(Int64[] /*gf*/ o, Int32 oOffset)
        {
            for (var i = 0; i < 16; ++i)
            {
                o[oOffset + i] += (1 << 16);
                Int64 c = o[oOffset + i] >> 16;
                o[oOffset + (i + 1) * (i < 15 ? 1 : 0)] += c - 1 + 37 * (c - 1) * (i == 15 ? 1 : 0);
                o[oOffset + i] -= c << 16;
            }
        }

        private static void Sel25519(Int64[] /*gf*/ p, Int64[] /*gf*/ q, Int32 b)
        {
            Int64 t, c = ~(b - 1);
            for (var i = 0; i < 16; ++i)
            {
                t = c & (p[i] ^ q[i]);
                p[i] ^= t;
                q[i] ^= t;
            }
        }

        private static void Pack25519(Byte[] o, Int64[] /*gf*/ n, Int32 nOffset)
        {
            Int32 b = 0, i, j;
            Int64[] /*gf*/ m = new Int64[GF_LEN], t = new Int64[GF_LEN];

            for (i = 0; i < 16; ++i)
            {
                t[i] = n[nOffset + i];
            }

            Car25519(t, 0);
            Car25519(t, 0);
            Car25519(t, 0);

            for (j = 0; j < 2; ++j)
            {
                m[0] = t[0] - 0xffed;

                for (i = 1; i < 15; i++)
                {
                    m[i] = t[i] - 0xffff - ((m[i - 1] >> 16) & 1);
                    m[i - 1] &= 0xffff;
                }

                m[15] = t[15] - 0x7fff - ((m[14] >> 16) & 1);
                b = (Int32)((m[15] >> 16) & 1);
                m[14] &= 0xffff;
                Sel25519(t, m, 1 - b);
            }

            for (i = 0; i < 16; ++i)
            {
                o[2 * i] = (Byte)t[i];
                o[2 * i + 1] = (Byte)(t[i] >> 8);
            }
        }

        private static Int32 Neq25519(Int64[] /*gf*/ a, Int64[] /*gf*/ b)
        {
            Byte[] c = new Byte[32], d = new Byte[32];
            Pack25519(c, a, 0);
            Pack25519(d, b, 0);
            return CryptoVerify32(c, d);
        }

        private static Byte Par25519(Int64[] /*gf*/ a)
        {
            Byte[] d = new Byte[32];

            Pack25519(d, a, 0);

            return (Byte)(d[0] & 1);
        }

        private static void Unpack25519(Int64[] /*gf*/ o, Byte[] n)
        {
            for (var i = 0; i < 16; ++i)
            {
                o[i] = (0xff & n[2 * i]) + ((0xffL & n[2 * i + 1]) << 8);
            }

            o[15] &= 0x7fff;
        }

        private static void A(Int64[] /*gf*/ o, Int64[] /*gf*/ a, Int64[] /*gf*/ b)
        {
            for (var i = 0; i < 16; ++i)
            {
                o[i] = a[i] + b[i];
            }
        }

        private static void Z(Int64[] /*gf*/ o, Int64[] /*gf*/ a, Int64[] /*gf*/ b)
        {
            for (var i = 0; i < 16; ++i)
            {
                o[i] = a[i] - b[i];
            }
        }

        private static void M(Int64[] /*gf*/ o, Int32 oOffset, Int64[] /*gf*/ a, Int32 aOffset, Int64[] /*gf*/ b, Int32 bOffset)
        {
            Int64[] t = new Int64[31];

            for (var i = 0; i < 31; ++i)
            {
                t[i] = 0;
            }

            for (var i = 0; i < 16; ++i)
            {
                for (var j = 0; j < 16; ++j)
                {
                    t[i + j] += a[aOffset + i] * b[bOffset + j];
                }
            }

            for (var i = 0; i < 15; ++i)
            {
                t[i] += 38 * t[i + 16];
            }

            for (var i = 0; i < 16; ++i)
            {
                o[oOffset + i] = t[i];
            }

            Car25519(o, oOffset);
            Car25519(o, oOffset);
        }

        private static void S(Int64[] /*gf*/ o, Int64[] /*gf*/ a)
        {
            M(o, 0, a, 0, a, 0);
        }

        private static void Inv25519(Int64[] /*gf*/ o, Int32 oOffset, Int64[] /*gf*/ i, Int32 iOffset)
        {
            Int64[] /*gf*/ c = new Int64[GF_LEN];

            for (var a = 0; a < 16; ++a)
            {
                c[a] = i[iOffset + a];
            }

            for (var a = 253; a >= 0; a--)
            {
                S(c, c);
                if (a != 2 && a != 4)
                {
                    M(c, 0, c, 0, i, iOffset);
                }
            }

            for (var a = 0; a < 16; ++a)
            {
                o[oOffset + a] = c[a];
            }
        }

        private static void Pow2523(Int64[] /*gf*/ o, Int64[] /*gf*/ i)
        {
            Int64[] /*gf*/ c = new Int64[GF_LEN];

            for (var a = 0; a < 16; ++a)
            {
                c[a] = i[a];
            }

            for (var a = 250; a >= 0; a--)
            {
                S(c, c);

                if (a != 1)
                {
                    M(c, 0, c, 0, i, 0);
                }
            }

            for (var a = 0; a < 16; ++a)
            {
                o[a] = c[a];
            }
        }

        #endregion

        #region Ed25519
        
        private static void Pack(Byte[] r, Int64[][] /*gf*/ p/*[4]*/)
        {
            Int64[] /*gf*/ tx = new Int64[GF_LEN], ty = new Int64[GF_LEN], zi = new Int64[GF_LEN];

            Inv25519(zi, 0, p[2], 0);
            M(tx, 0, p[0], 0, zi, 0);
            M(ty, 0, p[1], 0, zi, 0);

            Pack25519(r, ty, 0);

            r[31] ^= (Byte)(Par25519(tx) << 7);
        }

        private static void Add(Int64[][] p, Int64[][] q)
        {
            Int64[] a = new Int64[GF_LEN],
                    b = new Int64[GF_LEN],
                    c = new Int64[GF_LEN],
                    d = new Int64[GF_LEN],
                    t = new Int64[GF_LEN],
                    e = new Int64[GF_LEN],
                    f = new Int64[GF_LEN],
                    g = new Int64[GF_LEN],
                    h = new Int64[GF_LEN]
                    ;

            Z(a, p[1], p[0]);
            Z(t, q[1], q[0]);
            M(a, 0, a, 0, t, 0);
            A(b, p[0], p[1]);
            A(t, q[0], q[1]);
            M(b, 0, b, 0, t, 0);
            M(c, 0, p[3], 0, q[3], 0);
            M(c, 0, c, 0, D2, 0);
            M(d, 0, p[2], 0, q[2], 0);
            A(d, d, d);
            Z(e, b, a);
            Z(f, d, c);
            A(g, d, c);
            A(h, b, a);

            M(p[0], 0, e, 0, f, 0);
            M(p[1], 0, h, 0, g, 0);
            M(p[2], 0, g, 0, f, 0);
            M(p[3], 0, e, 0, h, 0);
        }

        private static void Cswap(Int64[][] /*gf*/ p/*[4]*/, Int64[][] /*gf*/ q/*[4]*/, Byte b)
        {
            for (var i = 0; i < 4; i++)
                Sel25519(p[i], q[i], b & 0xff);
        }
        
        private static void Scalarmult(Int64[][] /*gf*/ p /*[4]*/ , Int64[][] /*gf*/ q /*[4]*/, Byte[] s, Int32 sOffset)
        {
            Set25519(p[0], GF0);
            Set25519(p[1], GF1);
            Set25519(p[2], GF1);
            Set25519(p[3], GF0);

            for (var i = 255; i >= 0; --i)
            {
                Byte b = (Byte)(((0xff & s[sOffset + i / 8]) >> (i & 7)) & 1);
                Cswap(p, q, b);
                Add(q, p);
                Add(p, p);
                Cswap(p, q, b);
            }
        }

        private static void Scalarbase(Int64[][] /*gf*/ p/*[4]*/, Byte[] s, Int32 sOffset)
        {
            Int64[][] /*gf*/ q = new Int64[4][] { new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN], new Int64[GF_LEN] };
            Set25519(q[0], X);
            Set25519(q[1], Y);
            Set25519(q[2], GF1);
            M(q[3], 0, X, 0, Y, 0);
            Scalarmult(p, q, s, sOffset);
        }


        #endregion


        private static Int32 Vn(Byte[] x, Byte[] y, Int32 n, Int32 xOffset = 0)
        {
            Int32 d = 0;
            for (var i = 0; i < n; ++i) d |= x[i + xOffset] ^ y[i];
            return (1 & ((d - 1) >> 8)) - 1;
        }

        private static Int32 CryptoVerify16(Byte[] x, Byte[] y, Int32 xOffset)
        {
            return Vn(x, y, 16, xOffset);
        }

        private static Int32 CryptoVerify32(Byte[] x, Byte[] y)
        {
            return Vn(x, y, 32);
        }
        
        private static void ModL(Byte[] r, Int32 rOffset, Int64[] x/*[64]*/)
        {
            Int64 carry;
            Int32 i, j;
            for (i = 63; i >= 32; --i)
            {
                carry = 0;
                for (j = i - 32; j < i - 12; ++j)
                {
                    x[j] += carry - 16 * x[i] * L[j - (i - 32)];
                    carry = (x[j] + 128) >> 8;
                    x[j] -= carry << 8;
                }
                x[j] += carry;
                x[i] = 0;
            }
            carry = 0;

            for (j = 0; j < 32; ++j)
            {
                x[j] += carry - (x[31] >> 4) * L[j];
                carry = x[j] >> 8;
                x[j] &= 255;
            }

            for (j = 0; j < 32; ++j)
            {
                x[j] -= carry * L[j];
            }

            for (i = 0; i < 32; ++i)
            {
                x[i + 1] += x[i] >> 8;
                r[rOffset + i] = (Byte)(x[i] & 255);
            }
        }

        private static void Reduce(Byte[] r)
        {
            Int64[] x = new Int64[64];
            for (int i = 0; i < 64; i++)
            {
                x[i] = 0xff & r[i];
            }

            for (int i = 0; i < 64; ++i)
            {
                r[i] = 0;
            }

            ModL(r, 0, x);
        }

        private static Int32 Unpackneg(Int64[][] /*gf*/ r/*[4]*/, Byte[] p/*[32]*/)
        {
            Int64[] /*gf*/ t = new Int64[GF_LEN],
                chk = new Int64[GF_LEN],
                num = new Int64[GF_LEN],
                den = new Int64[GF_LEN],
                den2 = new Int64[GF_LEN],
                den4 = new Int64[GF_LEN],
                den6 = new Int64[GF_LEN];

            Set25519(r[2], GF1);
            Unpack25519(r[1], p);
            S(num, r[1]);
            M(den, 0, num, 0, D, 0);
            Z(num, num, r[2]);
            A(den, r[2], den);

            S(den2, den);
            S(den4, den2);
            M(den6, 0, den4, 0, den2, 0);
            M(t, 0, den6, 0, num, 0);
            M(t, 0, t, 0, den, 0);

            Pow2523(t, t);
            M(t, 0, t, 0, num, 0);
            M(t, 0, t, 0, den, 0);
            M(t, 0, t, 0, den, 0);
            M(r[0], 0, t, 0, den, 0);

            S(chk, r[0]);
            M(chk, 0, chk, 0, den, 0);
            if (Neq25519(chk, num) != 0)
            {
                M(r[0], 0, r[0], 0, I, 0);
            }

            S(chk, r[0]);
            M(chk, 0, chk, 0, den, 0);
            if (Neq25519(chk, num) != 0)
            {
                return -1;
            }

            if (Par25519(r[0]) == ((0xff & p[31]) >> 7))
            {
                Z(r[0], GF0, r[0]);
            }

            M(r[3], 0, r[0], 0, r[1], 0);

            return 0;
        }

        #endregion

        #region Secret-key cryptography

        #endregion

        #region Low-level functions:

        /// <summary>
        /// crypto_hash is currently an implementation of SHA-512.
        /// </summary>
        /// <param name="hash">hash for message</param>
        /// <param name="message">message to calculate the hash</param>
        /// <param name="n"></param>
        /// <returns>
        ///     0 for success or -1 for failure
        /// </returns>
        public static Int32 CryptoHash(Byte[] hash, Byte[] message, Int32 n)
        {
            Byte[] h = new Byte[HashBytes];
            Byte[] x = new Byte[256];
            Int32 b = n;

            for (var i = 0; i < HashBytes; i++)
            {
                h[i] = iv[i];
            }

            CryptoHashBlocks(h, message, n);

            for (var i = 0; i < HashBytes; i++)
            {
                for (var j = 0; j < message.Length; j++)
                {
                    message[j] += (Byte)n;
                }

                n &= 127;

                for (var j = 0; j < message.Length; j++)
                {
                    message[j] -= (Byte)n;
                }
            }

            for (var i = 0; i < 256; i++)
            {
                x[i] = 0;
            }

            for (var i = 0; i < n; i++)
            {
                x[i] = message[i];
            }

            x[n] = 128;

            n = ((n < 112) ? 256 - 128 * 1 : 256 - 128 * 0);
            x[n - 9] = (Byte)(b >> 61);

            Ts64(x, (UInt64)b << 3, n - 8);

            CryptoHashBlocks(h, x, n);

            for (var i = 0; i < HashBytes; i++)
            {
                hash[i] = h[i];
            }

            return 0;
        }


        private static Int32 CryptoHashBlocks(Byte[] x, Byte[] m, Int32 n)
        {
            UInt64[] z = new UInt64[8];
            UInt64[] b = new UInt64[8];
            UInt64[] a = new UInt64[8];
            UInt64[] w = new UInt64[16];
            UInt64 t = 0;

            for (var i = 0; i < 8; i++)
            {
                z[i] = a[i] = Dl64(x, 8 * i);
            }

            while (n >= 128)
            {
                for (var i = 0; i < 16; i++)
                {
                    w[i] = Dl64(m, 8 * i);
                }

                for (var i = 0; i < 80; i++)
                {
                    for (var j = 0; j < 8; j++)
                    {
                        b[j] = a[j];
                    }

                    t = a[7] + Sigma1(a[4]) + Ch(a[4], a[5], a[6]) + K[i] + w[i % 16];
                    b[7] = t + Sigma0(a[0]) + Maj(a[0], a[1], a[2]);
                    b[3] += t;
                    for (var j = 0; j < 8; j++)
                    {
                        a[(j + 1) % 8] = b[j];
                    }

                    if (i % 16 == 15)
                        for (var j = 0; j < 16; j++)
                        {
                            w[j] += w[(j + 9) % 16] + sigma0(w[(j + 1) % 16]) + sigma1(w[(j + 14) % 16]);
                        }
                }

                for (var i = 0; i < 8; i++)
                {
                    a[i] += z[i]; z[i] = a[i];
                }

                for (var i = 0; i < m.Length; i++)
                {
                    m[i] += 128;
                }

                n -= 128;
            }

            for (var i = 0; i < 8; i++)
            {
                Ts64(x, z[i], 8 * i);
            }

            return n;
        }


        #endregion
        
        /// <summary>
        /// crypto_onetimeauth is crypto_onetimeauth_poly1305, an authenticator specified in "Cryptography in NaCl", Section 9. 
        /// This authenticator is proven to meet the standard notion of unforgeability after a single message. 
        /// </summary>
        /// <param name="pout"></param>
        /// <param name="poutOffset"></param>
        /// <param name="m"></param>
        /// <param name="mOffset"></param>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Int32 CryptoOnetimeAuth(Byte[] pout, Int32 poutOffset, Byte[] m, Int64 mOffset, Int64 n, Byte[] k)
        {
            Int32 i = 0, j = 0, u = 0, s = 0;
            Int32[] x = new Int32[17], r = new Int32[17], h = new Int32[17], c = new Int32[17], g = new Int32[17];

            for (j = 0; j < 17; ++j)
            {
                r[j] = 0;
                h[j] = 0;
            }

            for (j = 0; j < 16; ++j)
            {
                r[j] = 0xff & k[j];
            }

            r[3] &= 15;
            r[4] &= 252;
            r[7] &= 15;
            r[8] &= 252;
            r[11] &= 15;
            r[12] &= 252;
            r[15] &= 15;

            while (n > 0)
            {
                for (j = 0; j < 17; ++j)
                    c[j] = 0;

                for (j = 0; (j < 16) && (j < n); ++j)
                    c[j] = 0xff & m[mOffset + j];

                c[j] = 1;
                mOffset += (Int64)j; n -= (Int64)j;
                Add1305(h, c);

                for (i = 0; i < 17; ++i)
                {
                    x[i] = 0;

                    for (j = 0; j < 17; ++j)
                    {
                        x[i] += h[j] * ((j <= i) ? r[i - j] : 320 * r[i + 17 - j]);
                    }
                }

                for (i = 0; i < 17; ++i)
                {
                    h[i] = x[i];
                }

                u = 0;
                for (j = 0; j < 16; ++j)
                {
                    u += h[j];
                    h[j] = u & 255;
                    u >>= 8;
                }

                u += h[16];
                h[16] = u & 3;
                u = 5 * (u >> 2);

                for (j = 0; j < 16; ++j)
                {
                    u += h[j];
                    h[j] = u & 255;
                    u >>= 8;
                }

                u += h[16];
                h[16] = u;
            }

            for (j = 0; j < 17; ++j)
            {
                g[j] = h[j];
            }

            Add1305(h, Minusp);
            s = -(h[16] >> 7);

            for (j = 0; j < 17; ++j)
            {
                h[j] ^= s & (g[j] ^ h[j]);
            }

            for (j = 0; j < 16; ++j)
            {
                c[j] = 0xff & k[j + 16];
            }


            c[16] = 0;
            Add1305(h, c);

            for (j = 0; j < 16; ++j)
            {
                pout[poutOffset + j] = (Byte)h[j];
            }

            return 0;
        }

        public static Int32 CryptoOnetimeauthVerify(Byte[] h, Int32 hoffset, Byte[] m, Int64 mOffset, Int64 n, Byte[] k)
        {
            Byte[] x = new Byte[16];
            CryptoOnetimeAuth(x, 0, m, mOffset, n, k);
            return CryptoVerify16(h, x, hoffset);
        }

        /// <summary>
        /// The crypto_secretbox function encrypts and authenticates a message
        /// </summary>
        /// <param name="cipheredMessage"></param>
        /// <param name="message"></param>
        /// <param name="nonce"></param>
        /// <param name="secretkey"></param>
        /// <returns></returns>
        public static Byte[] CryptoSecretBox(Byte[] message, Byte[] nonce, Byte[] secretkey)
        {
            Byte[] paddedMessage = new Byte[message.Length + BoxZeroBytes];
            Byte[] boxMessage = new Byte[message.Length];
            Byte[] cMessage = new Byte[message.Length + BoxBoxZeroBytes];

            Array.Copy(message, 0, paddedMessage, BoxZeroBytes, message.Length);

            var ciphered = CryptoStreamXor(paddedMessage, nonce, secretkey);

            if (ciphered.Length == 0)
            {
                throw new InvalidCipherTextException();
            }

            if (CryptoOnetimeAuth(ciphered, 16, ciphered, 32, paddedMessage.Length - 32, ciphered) != 0)
            {
                throw new InvalidCipherTextException();
            }

            for (var i = 0; i < BoxBoxZeroBytes; ++i)
            {
                ciphered[i] = 0;
            }

            Array.Copy(ciphered, BoxBoxZeroBytes, cMessage, 0, ciphered.Length - BoxBoxZeroBytes);

            return cMessage;
        }

        /// <summary>
        /// The CryptoSecretboxOpen function verifies and decrypts a ciphertext
        /// </summary>
        /// <param name="paddedMessage"></param>
        /// <param name="cipheredMessage"></param>
        /// <param name="nonce"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static Byte[] CryptoSecretBoxOpen(Byte[] cipheredMessage, Byte[] nonce, Byte[] secretKey)
        {
            Byte[] x = new Byte[32];
            Byte[] boxCipheredMessage = new Byte[cipheredMessage.Length + BoxBoxZeroBytes];
            Byte[] message = new Byte[cipheredMessage.Length - BoxBoxZeroBytes];

            Array.Copy(cipheredMessage, 0, boxCipheredMessage, BoxBoxZeroBytes, cipheredMessage.Length);

            if (boxCipheredMessage.Length < BoxZeroBytes)
            {
                throw new InvalidCipherTextException();
            }

            var nonceKey = CryptoStream(x, 32, nonce, secretKey);

            if (nonceKey.Length == 0)
            {
                throw new InvalidCipherTextException();
            }

            if (CryptoOnetimeauthVerify(boxCipheredMessage, BoxBoxZeroBytes, boxCipheredMessage, BoxZeroBytes, boxCipheredMessage.Length - BoxZeroBytes, nonceKey) != 0)
            {
                throw new InvalidCipherTextException();
            }

            var decMessage = CryptoStreamXor(boxCipheredMessage, nonce, secretKey);

            Array.Copy(decMessage, BoxZeroBytes, message, 0, message.Length);

            return message;
        }

        /// <summary>
        /// generate number of random bytes equal to array size.
        /// </summary>
        /// <param name="d">generated random byte</param>
        public static void RandomBytes(Byte[] d)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(d);
            }
        }

        private static UInt64 R(UInt64 x, int c) { return (x >> c) | (x << (64 - c)); }
        
        private static UInt64 Ch(UInt64 x, UInt64 y, UInt64 z) { return (x & y) ^ (~x & z); }
        
        private static UInt64 Maj(UInt64 x, UInt64 y, UInt64 z) { return (x & y) ^ (x & z) ^ (y & z); }
        
        private static UInt64 Sigma0(UInt64 x) { return R(x, 28) ^ R(x, 34) ^ R(x, 39); }
        
        private static UInt64 Sigma1(UInt64 x) { return R(x, 14) ^ R(x, 18) ^ R(x, 41); }
        
        private static UInt64 sigma0(UInt64 x) { return R(x, 1) ^ R(x, 8) ^ (x >> 7); }
        
        private static UInt64 sigma1(UInt64 x) { return R(x, 19) ^ R(x, 61) ^ (x >> 6); }

        private static Byte[] _0 = new Byte[16];
        
        private static Byte[] _9 = new Byte[32] { 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private const Int32 GF_LEN = 16;
        
        private static Int64[] GF = new Int64[GF_LEN];
        
        private static Int64[] GF0 = new Int64[GF_LEN];
        
        private static Int64[] GF1 = new Int64[GF_LEN] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static Int64[] _121665 = new Int64[GF_LEN] { 0xDB41, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        
        private static Int64[] D = new Int64[] { 0x78a3, 0x1359, 0x4dca, 0x75eb, 0xd8ab, 0x4141, 0x0a4d, 0x0070, 0xe898, 0x7779, 0x4079, 0x8cc7, 0xfe73, 0x2b6f, 0x6cee, 0x5203 };
        
        private static Int64[] D2 = new Int64[] { 0xf159, 0x26b2, 0x9b94, 0xebd6, 0xb156, 0x8283, 0x149a, 0x00e0, 0xd130, 0xeef3, 0x80f2, 0x198e, 0xfce7, 0x56df, 0xd9dc, 0x2406 };
        
        private static Int64[] X = new Int64[] { 0xd51a, 0x8f25, 0x2d60, 0xc956, 0xa7b2, 0x9525, 0xc760, 0x692c, 0xdc5c, 0xfdd6, 0xe231, 0xc0a4, 0x53fe, 0xcd6e, 0x36d3, 0x2169 };
        
        private static Int64[] Y = new Int64[] { 0x6658, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666, 0x6666 };
        
        private static Int64[] I = new Int64[] { 0xa0b0, 0x4a0e, 0x1b27, 0xc4ee, 0xe478, 0xad2f, 0x1806, 0x2f43, 0xd7a7, 0x3dfb, 0x0099, 0x2b4d, 0xdf0b, 0x4fc1, 0x2480, 0x2b83 };
        
        private static UInt64[] K = new UInt64[80]
        {
          0x428a2f98d728ae22, 0x7137449123ef65cd, 0xb5c0fbcfec4d3b2f, 0xe9b5dba58189dbbc,
          0x3956c25bf348b538, 0x59f111f1b605d019, 0x923f82a4af194f9b, 0xab1c5ed5da6d8118,
          0xd807aa98a3030242, 0x12835b0145706fbe, 0x243185be4ee4b28c, 0x550c7dc3d5ffb4e2,
          0x72be5d74f27b896f, 0x80deb1fe3b1696b1, 0x9bdc06a725c71235, 0xc19bf174cf692694,
          0xe49b69c19ef14ad2, 0xefbe4786384f25e3, 0x0fc19dc68b8cd5b5, 0x240ca1cc77ac9c65,
          0x2de92c6f592b0275, 0x4a7484aa6ea6e483, 0x5cb0a9dcbd41fbd4, 0x76f988da831153b5,
          0x983e5152ee66dfab, 0xa831c66d2db43210, 0xb00327c898fb213f, 0xbf597fc7beef0ee4,
          0xc6e00bf33da88fc2, 0xd5a79147930aa725, 0x06ca6351e003826f, 0x142929670a0e6e70,
          0x27b70a8546d22ffc, 0x2e1b21385c26c926, 0x4d2c6dfc5ac42aed, 0x53380d139d95b3df,
          0x650a73548baf63de, 0x766a0abb3c77b2a8, 0x81c2c92e47edaee6, 0x92722c851482353b,
          0xa2bfe8a14cf10364, 0xa81a664bbc423001, 0xc24b8b70d0f89791, 0xc76c51a30654be30,
          0xd192e819d6ef5218, 0xd69906245565a910, 0xf40e35855771202a, 0x106aa07032bbd1b8,
          0x19a4c116b8d2d0c8, 0x1e376c085141ab53, 0x2748774cdf8eeb99, 0x34b0bcb5e19b48a8,
          0x391c0cb3c5c95a63, 0x4ed8aa4ae3418acb, 0x5b9cca4f7763e373, 0x682e6ff3d6b2b8a3,
          0x748f82ee5defb2fc, 0x78a5636f43172f60, 0x84c87814a1f0ab72, 0x8cc702081a6439ec,
          0x90befffa23631e28, 0xa4506cebde82bde9, 0xbef9a3f7b2c67915, 0xc67178f2e372532b,
          0xca273eceea26619c, 0xd186b8c721c0c207, 0xeada7dd6cde0eb1e, 0xf57d4f7fee6ed178,
          0x06f067aa72176fba, 0x0a637dc5a2c898a6, 0x113f9804bef90dae, 0x1b710b35131c471b,
          0x28db77f523047d84, 0x32caab7b40c72493, 0x3c9ebe0a15c9bebc, 0x431d67c49c100d4c,
          0x4cc5d4becb3e42b6, 0x597f299cfc657e2a, 0x5fcb6fab3ad6faec, 0x6c44198c4a475817
        };
        
        private static Byte[] iv = new Byte[64]
        {
          0x6a,0x09,0xe6,0x67,0xf3,0xbc,0xc9,0x08,
          0xbb,0x67,0xae,0x85,0x84,0xca,0xa7,0x3b,
          0x3c,0x6e,0xf3,0x72,0xfe,0x94,0xf8,0x2b,
          0xa5,0x4f,0xf5,0x3a,0x5f,0x1d,0x36,0xf1,
          0x51,0x0e,0x52,0x7f,0xad,0xe6,0x82,0xd1,
          0x9b,0x05,0x68,0x8c,0x2b,0x3e,0x6c,0x1f,
          0x1f,0x83,0xd9,0xab,0xfb,0x41,0xbd,0x6b,
          0x5b,0xe0,0xcd,0x19,0x13,0x7e,0x21,0x79
        };

        private static Byte[] Sigma = Encoding.ASCII.GetBytes("expand 32-byte k");

        private static Int32[] Minusp = new Int32[17] { 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 252 };

        private static Int64[] L = new Int64[32]
        {
            0xed, 0xd3, 0xf5, 0x5c, 0x1a, 0x63, 0x12, 0x58,
            0xd6, 0x9c, 0xf7, 0xa2, 0xde, 0xf9, 0xde, 0x14,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0x10
        };

        private static UInt32 L32(UInt32 x, Int32 c) { return (x << c) | ((x & 0xffffffff) >> (32 - c)); }

        private static UInt32 Ld32(Byte[] x, Int32 offset = 0)
        {
            UInt32 u = x[3 + offset];
            u = (u << 8) | x[2 + offset];
            u = (u << 8) | x[1 + offset];
            return (u << 8) | x[0 + offset];
        }

        private static UInt64 Dl64(Byte[] x, Int64 offset)
        {
            UInt64 u = 0;
            for (var i = 0; i < 8; ++i) u = (u << 8) | x[i + offset];
            return u;
        }

        private static void St32(Byte[] x, UInt32 u, Int32 offset = 0)
        {
            for (var i = 0; i < 4; ++i)
            {
                x[i + offset] = (Byte)u; u >>= 8;
            }
        }

        private static void Ts64(Byte[] x, UInt64 u, Int32 offset = 0)
        {
            for (var i = 7; i >= 0; --i)
            {
                x[i + offset] = (Byte)u; u >>= 8;
            }
        }

        private static Byte[] CryptoCoreSalsa20(Byte[] pout, Byte[] pin, Byte[] k, Byte[] c)
        {
            return Core(pout, pin, k, c, false);
        }

        private static Byte[] CryptoStreamSalsa20Xor(Byte[] message, Byte[] nonce, Int32 nOffset, Byte[] secretKey)
        {
            Int32 i = 0;
            Byte[] z = new Byte[16];
            Byte[] x = new Byte[64];
            Byte[] cipheredMessage = new Byte[message.Length];
            Int64 messageSize = message.Length;
            UInt32 u = 0;

            for (i = 0; i < 8; ++i)
            {
                z[i] = nonce[nOffset + i];
            }

            Int32 cOffset = 0;
            Int32 mOffset = 0;

            while (messageSize >= 64)
            {
                CryptoCoreSalsa20(x, z, secretKey, Sigma);

                for (i = 0; i < 64; ++i)
                {
                    cipheredMessage[cOffset + i] = (Byte)((message != null ? message[mOffset + i] : 0) ^ x[i]);
                }

                u = 1;
                for (i = 8; i < 16; ++i)
                {
                    u += (UInt32)0xff & z[i];
                    z[i] = (Byte)u;
                    u >>= 8;
                }

                messageSize -= 64;
                cOffset += 64;
                if (message != null)
                {
                    mOffset += 64;
                }
            }

            if (messageSize != 0)
            {
                CryptoCoreSalsa20(x, z, secretKey, Sigma);

                for (i = 0; i < messageSize; i++)
                {
                    cipheredMessage[cOffset + i] = (Byte)((message != null ? message[mOffset + i] : 0) ^ x[i]);
                }
            }

            return cipheredMessage;
        }

        private static Byte[] CryptoStreamSalsa20(Byte[] message, Byte[] nonce, Int32 nOffset, Byte[] secretKey)
        {
            return CryptoStreamSalsa20Xor(message, nonce, nOffset, secretKey);
        }

        private static Byte[] CryptoStream(Byte[] nonceKey, Int64 d, Byte[] nonce, Byte[] secretKey)
        {
            Byte[] s = CryptoCoreHSalsa20(nonce, secretKey, Sigma);
            return CryptoStreamSalsa20(nonceKey, nonce, 16, s);
        }

        private static Byte[] CryptoStreamXor(Byte[] message, Byte[] nonce, Byte[] secretKey)
        {
            Byte[] s = CryptoCoreHSalsa20(nonce, secretKey, Sigma);
            return CryptoStreamSalsa20Xor(message, nonce, 16, s);

        }

        private static void Add1305(Int32[] h, Int32[] c)
        {
            Int32 u = 0, j = 0;
            for (j = 0; j < 17; ++j)
            {
                u += h[j] + c[j];
                h[j] = u & 255;
                u >>= 8;
            }
        }

        public static byte[] Increment(byte[] nonce)
        {
            var inc = new byte[nonce.Length];
            short c = 1;
            for (var i = 0; i < nonce.Length; i++)
            {
                c += nonce[i];
                inc[i] = (byte) c;
                c >>= 8;
            }
            return inc;
        }
    }
}

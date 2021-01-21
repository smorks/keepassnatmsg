using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KeePassNatMsg.Utils
{
    public class Totp
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string Generate(string TotpSettings)
        {
            KeyUri settings = new KeyUri(new Uri(TotpSettings));
            var counter = GetCounter(settings.Period);
            byte[] codeInterval = BitConverter.GetBytes((ulong)counter);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(codeInterval);
            }
            byte[] key = Base32.Decode(settings.Secret);

            using (var hmac = GetHashAlgo(settings.Algorithm, key))
            {
                byte[] hash = hmac.ComputeHash(codeInterval); //Generates hash from key using counter.
                hmac.Clear(); //Clear hash instance securing the key.

                int start = hash[hash.Length - 1] & 0xf;
                byte[] totp = new byte[4];

                Array.Copy(hash, start, totp, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(totp);
                }

                if (settings.Digits.Equals("S"))
                {
                    return SteamEncoder(totp, 5);
                }

                return Rfc6238Encoder(totp, settings.Digits);
            }
        }

        private static long GetCounter(int period)
        {
            var elapsedSeconds = (long)Math.Floor((DateTime.UtcNow - UnixEpoch).TotalSeconds); //Compute current counter for current time.
            return elapsedSeconds / period; //Applies specified interval to computed counter.
        }

        private static HMAC GetHashAlgo(string algorithm, byte[] key)
        {
            switch (algorithm)
            {
                case "SHA1": return new HMACSHA1(key, true);
                case "SHA256": return new HMACSHA256(key);
                case "SHA512": return new HMACSHA512(key);
            }
            return new HMACMD5(key); // Never
        }

        /// <summary>
		/// Character set for authenticator code
		/// </summary>
		private static readonly char[] Steamchars = new char[] {
                '2', '3', '4', '5', '6', '7', '8', '9', 'B', 'C',
                'D', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q',
                'R', 'T', 'V', 'W', 'X', 'Y'};

        private static string Rfc6238Encoder(byte[] bytes, int length)
        {
            var fullcode = OTP2UInt(bytes);
            var mask = (uint)Math.Pow(10, length);

            return (fullcode % mask).ToString(new string('0', length));
        }

        private static string SteamEncoder(byte[] bytes, int length)
        {
            var fullcode = OTP2UInt(bytes);

            StringBuilder code = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                code.Append(Steamchars[fullcode % Steamchars.Length]);
                fullcode /= (uint)Steamchars.Length;
            }

            return code.ToString();
        }

        private static uint OTP2UInt(byte[] totp)
        {
            return BitConverter.ToUInt32(totp, 0) & 0x7fffffff;
        }
    }
}

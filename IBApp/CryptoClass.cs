using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace IBApp
{
    internal static class CryptoClass
    {
        private static readonly string key = "YlgOhnfOH335DIxAKYSCTVcsksvTRdCTo2I3FYeTq81gwaAme7cJWesKRYe9bj4XmOfdeQrMyeghIufCWxOatrmOUe8bvO1rWUCS";

        internal static byte[] HashSome (byte[] strings)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                Encoding encoder = Encoding.UTF8;
                return mySHA256.ComputeHash(strings.Concat(encoder.GetBytes(key)).ToArray());
            }
        }

        internal static byte[] GetSalt ()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[256];
                rng.GetNonZeroBytes(salt);
                return salt;
            }
        }
    }
}

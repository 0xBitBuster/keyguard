using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KeyGuard.Helpers
{
    public static class PasswordGenerator
    {
        const string alphanumericCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "0123456789" + 
            "!!##++--(())$$";

        public static string GeneratePassword(int length)
        {
            var characterArray = alphanumericCharacters.Distinct().ToArray();
            var bytes = new byte[length * 8];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);

                var result = new char[length];
                for (int i = 0; i < length; i++)
                {
                    ulong value = BitConverter.ToUInt64(bytes, i * 8);
                    result[i] = characterArray[value % (uint)characterArray.Length];
                }
             
                return new string(result);
            }
        }
    }
}

#region Imports

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using ECP.API;

#endregion
namespace ECP.API.Security
{
    internal static class ECPStringGenerator
    {
        #region Methods

        internal static string GenerateString(int length)
        {
            Random r = new Random();
            char[] pool = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N',
                            'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b',
                            'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3',
                            '4', '5', '6', '7', '8', '9'};
            string name = null;
            for (int i = 0; i < length; i++) { name += pool[r.Next(0, pool.Length - 1)]; }
            return name ?? (name = "Abstract" + r.Next(0, 8192).ToString());
        }

        #endregion
    }

    internal class ECPRandomNumberGenerator
    {
        #region Variables

        private static RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();

        #endregion
        #region Methods

        public uint NextUInt32()
        {
            byte[] res = new byte[4];
            csp.GetBytes(res);
            return BitConverter.ToUInt32(res, 0);
        }

        public int NextInt()
        {
            byte[] res = new byte[4];
            csp.GetBytes(res);
            return BitConverter.ToInt32(res, 0);
        }

        public Single NextSingle()
        {
            float numerator = NextUInt32();
            float denominator = uint.MaxValue;
            return numerator / denominator;
        }

        #endregion
    }
}

using System;
using System.Security.Cryptography;
using System.Text;

namespace Piipan.Match.Orchestrator
{
    static class LookupId
    {
        private const string Chars = "23456789BCDFGHJKLMNPQRSTVWXYZ";
        private const int Length = 7;

        /// <summary>
        /// Generate a deterministic alphanumeric ID from a given string.
        /// IDs are encoded using a limited alphabet and fixed length.
        /// </summary>
        /// <param name="value">The string value used to generate the ID</param>
        /// <returns>An alpha-numeric ID string</returns>
        public static string Generate(string value)
        {
            var idString = new StringBuilder();
            var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));

            // Only keep the first 32-bits from the 128-bit hash so as to fit into 7 encoded characters (29^7 > 2^32)
            long hashInt = BitConverter.ToUInt32(hashBytes, 0);

            // Encode using custom alphabet
            do
            {
                idString.Insert(0, Chars[(int)(hashInt % Chars.Length)]);
                hashInt = (long)(hashInt / Chars.Length);
            } while (hashInt > 0);

            // Returned IDs should always be `Length` length
            return idString.ToString().PadLeft(Length, Chars[0]);
        }
    }
}

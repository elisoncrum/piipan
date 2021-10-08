using System;
using System.Text.RegularExpressions;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Normalizes last name according to PPRL specifications.
    /// </summary>
    public class NameNormalizer : INameNormalizer
    {
        /// <summary>
        /// Public entrypoint for functionality
        /// </summary>
        /// <param name="lname">last name of individual, expects only ASCII characters</param>
        public string Run(string lname)
        {
            // Loud failure for non-ascii chars
            Regex nonasciirgx = new Regex(@"[^\x00-\x7F]");
            if (nonasciirgx.IsMatch(lname))
            {
                throw new ArgumentException("name must contain only ascii characters");
            }
            // Convert to lower case
            string result = lname.ToLower();
            // Replace hyphens with a space
            result = result.Replace("-", " ");
            // Replace multiple spaces with one space
            result = Regex.Replace(result, @"\s{2,}", " ");
            // Trim any spaces at the start and end of the last name
            char[] charsToTrim = { ' ' };
            result = result.Trim(charsToTrim);
            // Remove suffixes: roman numerals i-ix, variations of junior/senior
            result = Regex.Replace(result, @"(\s(?:ix|iv|v?i{0,3}|junior|jr\.|jr|jnr|senior|sr\.|sr|snr)$)", "");
            // Remove any character not an ASCII space(0x20) or not in range[a - z]
            result = Regex.Replace(result, @"[^a-z|\s]", "");
            // Validate that the resulting value is at least one ASCII character in length
            if (result.Length < 1) // not at least one char
            {
                throw new ArgumentException("normalized name must be at least 1 character long");
            }
            return result;
        }
    }
}

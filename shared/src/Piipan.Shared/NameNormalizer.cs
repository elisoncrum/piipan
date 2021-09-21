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
        /// <param name="lname">last name of individual</param>
        public string Run(string lname)
        {
            // loud failure of any non-ascii characters
            Regex nonasciirgx = new Regex(@"[^\x00-\x7F]");
            if (nonasciirgx.IsMatch(lname))
            {
                throw new ArgumentException("name must contain only ascii characters");
            }
            // Convert to lower case
            string result = lname.ToLower();
            // TODO: Remove any suffixes(e.g.; junior, jnr, jr, jr., iii, etc.)
            // Replace hyphens with a space
            result = result.Replace("-", " ");
            // TODO: Remove any character that is not an ASCII space(0x20) or in the range[a - z] (0x61 - 0x70)
            // Replace multiple spaces with one space
            result = Regex.Replace(result, @"\s{2,}", " ");
            // Trim any spaces at the start and end of the last name
            char[] charsToTrim = { ' ' };
            result = result.Trim(charsToTrim);
            // Validate that the resulting value is at least one ASCII character in length
            if (result.Length < 1) // not at least one char
            {
                throw new ArgumentException("normalized name must be at least 1 character long");
            }
            return result;
        }
    }
}

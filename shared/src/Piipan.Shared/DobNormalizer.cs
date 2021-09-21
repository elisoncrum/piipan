using System;
using System.Text.RegularExpressions;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Normalizes date of birth according to PPRL specifications.
    /// </summary>
    public class DobNormalizer : IDobNormalizer
    {
        /// <summary>
        /// Public entrypoint for class.
        /// </summary>
        /// <param name="dob">date of birth of individual</param>
        public string Run(string dob)
        {
            Regex iso8601rgx = new Regex(@"^\d{4}-\d{2}-\d{2}$");
            if (!iso8601rgx.IsMatch(dob))
            {
                throw new ArgumentException("dates must be in ISO 8601 format using a 4-digit year, a zero-padded month, and zero-padded day");
            }
            return dob;
        }
    }
}

using System;
using System.Text.RegularExpressions;

namespace Piipan.Shared.Deidentification
{
    public class SsnNormalizer : ISsnNormalizer
    {
        /// <summary>
        /// Public entrypoint for functionality.
        /// Normalizes social security number according to PPRL specifications.
        /// </summary>
        /// <param name="ssn">social security number of individual</param>
        public string Run(string ssn)
        {
            Regex ssnRgx = new Regex(@"^\d{3}-\d{2}-\d{4}$");
            if (!ssnRgx.IsMatch(ssn))
            {
                throw new ArgumentException("social security number must have a 3-digit area number, a 2-digit group number, and a 4-digit serial number, in this order, all separated by a hyphen");
            }
            return ssn;
        }
    }
}

using System;
using System.Text.RegularExpressions;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.Shared.Tests
{
    public class SsnNormalizerTests
    {
        public class RunTests
        {
            private SsnNormalizer _ssnNormalizer;

            public RunTests()
            {
                _ssnNormalizer = new SsnNormalizer();
            }

            // correct format
            [Theory]
            // Using valid ssn 987-65-4320 which is reserved
            // for use in advertisements
            // (ref: https://www.lexjansen.com/nesug/nesug07/ap/ap19.pdf)
            [InlineData("87-65-4320")] // missing Area digit
            [InlineData("987-6-4320")] // missing Group digit
            [InlineData("987-65-432")] // missing Serial digit
            [InlineData("987 65-4320")] // missing hyphen
            public void ThrowsExOnIncorrectFormat(string ssn)
            {
                Assert.Throws<ArgumentException>(() => _ssnNormalizer.Run(ssn));
            }

            // Area numbers 000, 666, and 900-999 are invalid
            [Theory (Skip = "not yet implemented")]
            [InlineData("000-11-1111")] // area number 000
            [InlineData("666-11-1111")] // area number 666
            [InlineData("901-11-1111")] // area number in 900-999 range
            public void ThrowsExOnInvalidAreaNumbers(string ssn)
            {
                Assert.Throws<ArgumentException>(() => _ssnNormalizer.Run(ssn));
            }

            // Group number 00 is invalid
            [Fact (Skip = "not yet implemented")]
            public void ThrowsExOnInvalidGroupNumber()
            {
                string ssn = "111-00-1111";
                Assert.Throws<ArgumentException>(() => _ssnNormalizer.Run(ssn));
            }

            // Serial number 0000 is invalid
            [Fact (Skip = "not yet implemented")]
            public void ThrowsExOnInvalidSerialNumber()
            {
                string ssn = "111-11-0000";
                Assert.Throws<ArgumentException>(() => _ssnNormalizer.Run(ssn));
            }
        }
    }
}

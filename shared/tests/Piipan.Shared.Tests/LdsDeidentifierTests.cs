using System;
using System.Text.RegularExpressions;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.Shared.Tests
{
    public class LdsDeidentifierTests
    {
        public class RunTests
        {
            public LdsDeidentifier _ldsDeidentifier;

            public RunTests()
            {
                _ldsDeidentifier = new LdsDeidentifier(
                    new NameNormalizer(),
                    new DobNormalizer(),
                    new SsnNormalizer(),
                    new LdsHasher()
                );
            }

            [Theory]
            [InlineData("Foobar", "2000-12-29", "111-11-1111")]
            [InlineData("von Neuman", "2020-01-01", "111-11-1111")]
            public void returnsValidDigest(string lname, string dob, string ssn)
            {
                string result = _ldsDeidentifier.Run(lname, dob, ssn);
                Assert.Matches("^[0-9a-f]{128}$", result);
                Assert.Equal(128, result.Length);
            }

            [Theory]
            [InlineData("Foobar", "1987-11-29", "000-00-000")] // missing serial number
            public void ThrowsExWhenInvalidInput(string lname, string dob, string ssn)
            {
                Assert.Throws<ArgumentException>(() => _ldsDeidentifier.Run(lname, dob, ssn));
            }

            [Fact]
            public void ReturnsExpectedResult()
            {
                string result = _ldsDeidentifier.Run("hopper", "1978-08-14", "987-65-4320");
                string expectedDigest = "ea192b480d93711a3d67ba5e9b9468f4f476564f54883b9e88c59bfbdf0ab3c9b3b5413b223b7430e733fd93e1bdb4993bced45386e670be64c0e75d5fa10bdd";
                Assert.Equal(expectedDigest, result);
            }
        }
    }
}

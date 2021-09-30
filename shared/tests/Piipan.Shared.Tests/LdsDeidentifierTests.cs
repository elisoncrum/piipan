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
            [InlineData("Foobar", "2000-12-29", "425-46-5417")]
            [InlineData("von Neuman", "2020-01-01", "195-26-4789")]
            public void returnsValidDigest(string lname, string dob, string ssn)
            {
                string result = _ldsDeidentifier.Run(lname, dob, ssn);
                Assert.Matches("^[0-9a-f]{128}$", result);
                Assert.Equal(128, result.Length);
            }

            [Theory]
            [InlineData("Foobar", "1987-11-29", "425-46-541")] // missing serial number
            public void ThrowsExWhenInvalidInput(string lname, string dob, string ssn)
            {
                Assert.Throws<ArgumentException>(() => _ldsDeidentifier.Run(lname, dob, ssn));
            }

            [Fact]
            public void ReturnsExpectedResult()
            {
                string result = _ldsDeidentifier.Run("hopper", "1978-08-14", "425-46-5417");
                string expectedDigest = "e733ee077eb82e13874a270bf170e3b999031c71eb5f0b47fc51c7cc677d0b8dd3b79615d79fa4ba2779c5fb9764b81aaa219dce20edb978a79903b647b5b714";
                Assert.Equal(expectedDigest, result);
            }
        }
    }
}

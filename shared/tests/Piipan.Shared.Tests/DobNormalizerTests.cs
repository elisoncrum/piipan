using System;
using System.Text.RegularExpressions;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.Shared.Tests
{
    public class DobNormalizerTests
    {

        public class RunTests
        {
            private DobNormalizer _dobNormalizer;

            public RunTests()
            {
                _dobNormalizer = new DobNormalizer();
            }

            [Theory]
            [InlineData("98-08-14")] // year is not fully specified
            [InlineData("5/15/2002")] // wrong value order, wrong separator character, value is not zero-padded
            [InlineData("2000-11-2")] // day is not zero-padded
            public void throwsExOnNonISO8601Dates(string date)
            {
                Assert.Throws<ArgumentException>(() => _dobNormalizer.Run(date));
            }

            [Theory (Skip = "not yet implemented")]
            [InlineData("2001-02-29")] // date does not exist on Gregorian calendar, 2001 is not a leap year
            public void ThrowsExOnNonGregorianDates(string date)
            {
                Assert.Throws<ArgumentException>(() => _dobNormalizer.Run(date));
            }

            [Fact (Skip = "not yet implemented")]
            public void ThrowsExOnDatesTooOld()
            {
                string date = DateTime.Now.AddYears(-131).ToString("yyyy-MM-dd");
                Assert.Throws<ArgumentException>(() => _dobNormalizer.Run(date));
            }

            [Theory]
            [InlineData("2000-11-02")]
            public void allowsISO8601Dates(string date)
            {
                var result = _dobNormalizer.Run(date);
                Assert.Equal(date, result);
            }
        }
    }
}

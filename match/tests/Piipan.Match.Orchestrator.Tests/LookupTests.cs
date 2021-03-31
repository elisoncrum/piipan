using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Piipan.Match.Orchestrator.Tests
{
    public class LookupTests
    {
        const string json = "{last: 'Last', first: 'First', middle: 'Middle', dob: '2020-01-01', ssn: '000-00-0000'}";
        const string json2 = "{last: 'last', first: 'first', middle: 'middle', dob: '2021-01-01', ssn: '000-11-1111'}";

        [Fact]
        public void Deterministic()
        {
            var str = json;
            var id = LookupId.Generate(str);

            Assert.Equal(id, LookupId.Generate(str));
        }

        [Theory]
        [InlineData(json)]
        [InlineData(json2)]
        [InlineData("ABC123")]
        [InlineData("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")]
        public void ConformsToLength(string value)
        {
            var id = LookupId.Generate(value);

            Assert.Equal(7, id.Length);
        }

        [Theory]
        [InlineData(json)]
        [InlineData(json2)]
        [InlineData("ABC123")]
        [InlineData("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")]
        public void ConformsToAlphabet(string value)
        {
            var disallowed = "01AEIOUabcdefghijklmnopqrstuvwxyz";
            var id = LookupId.Generate(value);

            foreach (char c in disallowed)
            {
                Assert.False(id.Contains(c));
            }
        }

        [Fact]
        public void Collisions()
        {
            var ids = new List<string>{
                LookupId.Generate(json),
                LookupId.Generate(json + " "),
                LookupId.Generate(json2),
                LookupId.Generate(json2 + " ")
            };

            Assert.Equal(ids.Distinct().Count(), ids.Count);
        }
    }
}

using System;
using System.Text.RegularExpressions;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.Shared.Tests
{
    public class NameNormalizerTests
    {
        public class RunTests
        {
            private readonly NameNormalizer _nameNormalizer;

            public RunTests()
            {
                _nameNormalizer = new NameNormalizer();
            }

            // This is a temporary exception which will be replaced by
            // a more thorough check of ascii characters in range
            // as described in /docs/pprl.md#last-name
            [Fact]
            public void throwsExceptionOnNonAscii()
            {
                Assert.Throws<ArgumentException>(() => _nameNormalizer.Run("garcía"));
                Assert.Throws<ArgumentException>(() => _nameNormalizer.Run("ståle"));
            }

            [Theory]
            [InlineData("Hopper")]
            [InlineData("FOO")]
            public void convertsToLowercase(string name)
            {
                string result = _nameNormalizer.Run(name);
                Assert.Matches(@"^[a-z|\s]+$", result);
            }

            [Theory (Skip = "not yet implemented")]
            [InlineData("maxwell junior")]
            [InlineData("two names junior")]
            [InlineData("maxwell jnr")]
            [InlineData("maxwell jr")]
            [InlineData("maxwell jr.")]
            [InlineData("maxwell iii")]
            [InlineData("maxwell iv")]
            [InlineData("maxwell v")]
            [InlineData("maxwell vi")]
            [InlineData("maxwell vii")]
            [InlineData("maxwell viii")]
            [InlineData("maxwell ix")]
            [InlineData("maxwell x")]
            [InlineData("maxwell xi")]
            [InlineData("maxwell xii")]
            [InlineData("maxwell xiii")]
            public void removesSuffixes(string name)
            {
                string result = _nameNormalizer.Run(name);
                Regex romanRgx = new Regex(@"(\s(?:ix|iv|v?i{0,3})$)"); // roman numerals i - ix
                Assert.DoesNotMatch(romanRgx, result);

                Regex jrRgx = new Regex(@"(\s(?:junior|jr.*|jnr)$)"); // variations of junior
                Assert.DoesNotMatch(jrRgx, result);
            }

            [Theory]
            [InlineData("barrable-tishauer")]
            [InlineData("barrable-tishauer-khan")]
            public void replacesHyphensWithSpace(string name)
            {
                string result = _nameNormalizer.Run(name);
                Regex rgx = new Regex(@"[-]");
                Assert.DoesNotMatch(rgx, result);
            }

            [Fact]
            public void replacesMultipleSpacesWithSingleSpace()
            {
                string result = _nameNormalizer.Run("quincy  chavez");
                Assert.Equal("quincy chavez", result);

                string lotsOfSpaces = _nameNormalizer.Run("quincy     chavez");
                Assert.Equal("quincy chavez", lotsOfSpaces);
            }

            [Fact]
            public void trimsWhitespace()
            {
                string result = _nameNormalizer.Run("  quincy chavez  ");
                Assert.Equal("quincy chavez", result);
            }

            // Remove any character that is not an ASCII space (0x20) or in the range [a-z] (0x61-0x70)
            [Theory (Skip = "not yet implemented")]
            [InlineData("foo.")]
            [InlineData("f'bar")]
            [InlineData("foobễr")]
            [InlineData("foo bar")] // non-ascii non-breaking space (on OSX: option + spacebar)
            public void removeAsciiNotInRange(string name)
            {
                string result = _nameNormalizer.Run(name);
                Regex rgx = new Regex(@"[^a-z|\x20]");
                Assert.DoesNotMatch(rgx, result);
            }

            [Fact]
            public void validatesAtleastOneAsciiChar()
            {
                Assert.Throws<ArgumentException>(() => _nameNormalizer.Run(""));
            }
        }
    }
}

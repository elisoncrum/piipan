using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Piipan.Shared.Deidentification.Tests
{
    public class NameNormalizerTests
    {
        private readonly NameNormalizer _nameNormalizer;

        public NameNormalizerTests()
        {
            _nameNormalizer = new NameNormalizer();
        }

        [Theory]
        [InlineData("garcía")]
        [InlineData("ståle")]
        [InlineData("foo bar")] // non-ascii non-breaking space (on OSX: option + spacebar)
        public void throwsExceptionOnNonAscii(string source)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _nameNormalizer.Run(source));
            Assert.Equal("name must contain only ascii characters", exception.Message);
        }

        [Theory]
        [InlineData("Foo", "foo")]
        [InlineData("FOO", "foo")]
        [InlineData("fOo", "foo")]
        public void Run_ConvertsToLowercase(string source, string expected)
        {
            Assert.Equal(expected, _nameNormalizer.Run(source));
        }

        [Theory]
        [InlineData("maxwell junior", "maxwell")]
        [InlineData("two names junior", "two names")]
        [InlineData("maxwell jnr", "maxwell")]
        [InlineData("maxwell jr", "maxwell")]
        [InlineData("maxwell jr.", "maxwell")]
        [InlineData("maxwell senior", "maxwell")]
        [InlineData("two names senior", "two names")]
        [InlineData("maxwell snr", "maxwell")]
        [InlineData("maxwell sr.", "maxwell")]
        [InlineData("maxwell iii", "maxwell")]
        [InlineData("maxwell iv", "maxwell")]
        [InlineData("maxwell v", "maxwell")]
        [InlineData("maxwell vi", "maxwell")]
        [InlineData("maxwell vii", "maxwell")]
        [InlineData("maxwell viii", "maxwell")]
        [InlineData("maxwell ix", "maxwell")]
        public void Run_RemovesSuffixes(string source, string expected)
        {
            Assert.Equal(expected, _nameNormalizer.Run(source));
        }

        [Theory]
        [InlineData("barrable-tishauer", "barrable tishauer")]
        [InlineData("barrable-tishauer-khan", "barrable tishauer khan")]
        public void Run_ReplacesHyphensWithSpace(string source, string expected)
        {
            Assert.Equal(expected, _nameNormalizer.Run(source));
        }

        [Theory]
        [InlineData("foo  bar", "foo bar")]
        [InlineData("foo      bar", "foo bar")]
        [InlineData("foo   bar   baz", "foo bar baz")]
        public void Run_ReplacesMultipleSpacesWithSingleSpace(string source, string expected)
        {
            Assert.Equal(expected, _nameNormalizer.Run(source));
        }

        [Theory]
        [InlineData("   foo bar   ", "foo bar")]
        public void Run_TrimsWhitespace(string source, string expected)
        {
            Assert.Equal(expected, _nameNormalizer.Run(source));
        }

        // Throw exception for any character that is not an
        // ASCII space (0x20) or not in range [a-z] (0x61-0x7a)
        [Theory ]
        [InlineData("foobar.", "foobar")]
        [InlineData("foobar,", "foobar")]
        [InlineData("foo'bar", "foobar")]
        public void Run_RemovesAsciiNotInRange(string source, string expected)
        {
            string result = _nameNormalizer.Run(source);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        public void Run_ValidatesAtleastOneAsciiChar(string source)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _nameNormalizer.Run(source));
            Assert.Equal("normalized name must be at least 1 character long", exception.Message);
        }

    }
}

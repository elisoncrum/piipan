using System.IO;
using System.Threading.Tasks;
using Piipan.Match.Func.Api.Parsers;
using Moq;
using Xunit;
using FluentValidation;
using System.Linq;

namespace Piipan.Match.Func.Api.Tests.Parsers
{
    public class OrchMatchRequestParserTests
    {
        [Fact]
        public async Task EmptyStreamThrows()
        {
            // Arrange
            var validator = Mock.Of<IValidator<OrchMatchRequest>>();
            var parser = new OrchMatchRequestParser(validator);

            // Act / Assert
            await Assert.ThrowsAsync<StreamParserException>(() => parser.Parse(BuildStream("")));
        }

        [Theory]
        [InlineData("{{")]
        [InlineData("<xml>")]
        [InlineData("{ data: 'foobar' }")]
        public async Task MalformedStreamThrows(string s)
        {
            // Arrange
            var validator = Mock.Of<IValidator<OrchMatchRequest>>();
            var parser = new OrchMatchRequestParser(validator);

            // Act / Assert
            await Assert.ThrowsAsync<StreamParserException>(() => parser.Parse(BuildStream(s)));
        }

        [Theory]
        [InlineData(@"{'data':[
            { 'lds_hash':'eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458' }
        ]}", 1)]
        [InlineData(@"{'data':[
            { 'lds_hash':'eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458' },
            { 'lds_hash':'eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458' },
        ]}", 2)]
        public async Task WellFormedStreamReturnsObject(string body, int count)
        {
            // Arrange
            var validator = Mock.Of<IValidator<OrchMatchRequest>>();
            var parser = new OrchMatchRequestParser(validator);

            // Act
            var request = await parser.Parse(BuildStream(body));

            // Assert
            Assert.NotNull(request);
            Assert.Equal(count, request.Data.Count());
            request.Data.ForEach(d =>
            {
                Assert.NotNull(d.LdsHash);
                Assert.Equal(128, d.LdsHash.Length);
            });
        }

        private Stream BuildStream(string s)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(s);
            sw.Flush();

            ms.Position = 0;

            return ms;
        }
    }
}
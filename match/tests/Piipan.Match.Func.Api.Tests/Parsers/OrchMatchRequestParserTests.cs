using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Piipan.Match.Func.Api.Models;
using Piipan.Match.Func.Api.Parsers;
using Moq;
using Xunit;
using FluentValidation;
using FluentValidation.Results;

namespace Piipan.Match.Func.Api.Tests.Parsers
{
    public class OrchMatchRequestParserTests
    {
        [Fact]
        public async Task EmptyStreamThrows()
        {
            // Arrange
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = Mock.Of<IValidator<OrchMatchRequest>>();
            var parser = new OrchMatchRequestParser(validator, logger);

            // Act / Assert
            await Assert.ThrowsAsync<StreamParserException>(() => parser.Parse(BuildStream("")));
        }

        [Theory]
        [InlineData("{{")]
        [InlineData("<xml>")]
        [InlineData("{ data: 'foobar' }")]
        [InlineData("{ data: []}")]
        [InlineData("{ data: [{}]}")]
        [InlineData("{ data: [{ssn: '000-00-0000'}]}")]
        public async Task MalformedStreamThrows(string s)
        {
            // Arrange
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = Mock.Of<IValidator<OrchMatchRequest>>();
            var parser = new OrchMatchRequestParser(validator, logger);

            // Act / Assert
            await Assert.ThrowsAsync<StreamParserException>(() => parser.Parse(BuildStream(s)));
        }

        [Theory]
        [InlineData(@"{ data: [{ lds_hash: 'abc' }]}", 1)] // invalid hash, but valid request
        [InlineData(@"{ data: [{ lds_hash: '' }]}", 1)] // empty hash, but valid request
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
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = new Mock<IValidator<OrchMatchRequest>>();
            validator
                .Setup(m => m.ValidateAsync(It.IsAny<OrchMatchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var parser = new OrchMatchRequestParser(validator.Object, logger);

            // Act
            var request = await parser.Parse(BuildStream(body));

            // Assert
            Assert.NotNull(request);
            Assert.Equal(count, request.Data.Count());
        }

        [Fact]
        public async Task ThrowsWhenValidatorThrows()
        {
            // Arrange
            var body = @"{'data':[
                { 'lds_hash':'eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458' }
            ]}";
        
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = new Mock<IValidator<OrchMatchRequest>>();
            validator
                .Setup(m => m.ValidateAsync(It.IsAny<OrchMatchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure("property", "missing")
                    }));

            var parser = new OrchMatchRequestParser(validator.Object, logger);

            // Act / Assert
            await Assert.ThrowsAsync<ValidationException>(() => parser.Parse(BuildStream(body)));
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
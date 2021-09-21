using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Authentication;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class OrchestratorApiRequestTests
    {
        [Fact]
        public async void TestMatch()
        {
            // arrange
            var mockResponse = @"{
                ""data"": {
                    ""results"": [
                        {
                            ""matches"": [
                                {
                                    ""lds_hash"": ""foobar"",
                                    ""state"": ""ea"",
                                    ""state_abbr"": ""ea""
                                }
                            ]
                        }
                    ],
                    ""errors"": []
                }
            }";
            var query = new MatchRequestRecord
            {
                LdsHash = "foobar"
            };
            var clientMock = new Mock<IAuthorizedApiClient>();
            clientMock
                .Setup(c => c.PostAsync(It.IsAny<Uri>(), It.IsAny<StringContent>()).Result)
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponse)
                });
            var api = new OrchestratorApiRequest(
                clientMock.Object,
                new Uri("https://localhost/"),
                new NullLogger<IndexModel>());

            // act
            var result = await api.Match(query);
            var match = result.Data.Results[0].Matches[0];

            // assert
            Assert.IsType<MatchResponse>(result);
            Assert.Single(result.Data.Results[0].Matches);
            Assert.Equal("foobar", match.LdsHash);
            Assert.Equal("ea", match.State);
        }
    }
}

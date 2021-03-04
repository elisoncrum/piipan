using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Moq;
using Moq.Protected;
using Piipan.Shared.Authentication;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class OrchestratorApiRequestTests
    {
        static Mock<ITokenProvider> MockTokenProvider(string value)
        {
            var token = new AccessToken(value, DateTimeOffset.Now);
            var mockTokenProvider = new Mock<ITokenProvider>();
            mockTokenProvider
                .Setup(t => t.RetrieveAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(token));

            return mockTokenProvider;
        }

        static Mock<HttpMessageHandler> MockHttpMessageHandler(string response)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
              .Protected()
              .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
              )
              .ReturnsAsync(new HttpResponseMessage()
              {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent(response)
              });

            return handlerMock;
        }
        [Fact]
        public async void TestQueryOrchestrator()
        {
            // arrange
            var mockResponse = @"{
                ""matches"": [
                    {
                        ""first"": ""Theodore"",
                        ""middle"": ""Carri"",
                        ""last"": ""Farrington""
                    }
                ]
            }";
            var handlerMock = MockHttpMessageHandler(mockResponse);
            var _apiRequest = new OrchestratorApiRequest();
            var query = new PiiRecord();

            // act
            var TestQueryResult = await _apiRequest.SendQuery("http://example.com", query);

            // assert
            Assert.Single(TestQueryResult);
            Assert.Equal("Theodore", TestQueryResult[0].FirstName);
            Assert.Equal("Farrington", TestQueryResult[0].LastName);
            handlerMock.Protected().Verify(
              "SendAsync",
              Times.AtLeastOnce(),
              ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
              ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}

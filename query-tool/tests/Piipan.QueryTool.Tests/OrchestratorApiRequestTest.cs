using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

        static AuthorizedJsonApiClient ConstructMocked(Mock<HttpMessageHandler> handler)
        {
            var mockTokenProvider = MockTokenProvider("|token|");
            var client = new HttpClient(handler.Object);
            var apiClient = new AuthorizedJsonApiClient(client, mockTokenProvider.Object);
            return apiClient;
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
            var mockApiClient = ConstructMocked(handlerMock);
            var query = new PiiRecord();
            var jsonString = JsonSerializer.Serialize(query);
            var requestBody = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var _apiRequest = new OrchestratorApiRequest(mockApiClient);

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

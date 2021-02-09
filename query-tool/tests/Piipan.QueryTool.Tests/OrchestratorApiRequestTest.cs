using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;

namespace Piipan.QueryTool.Tests
{
    public class OrchestratorApiRequestTests
    {
        [Fact]
        public async void TestQueryOrchestrator()
        {
            // arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var mockResponse = @"{
                ""matches"": [
                    {
                        ""first"": ""Theodore"",
                        ""middle"": ""Carri"",
                        ""last"": ""Farrington""
                    }
                ]
            }";
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
                  Content = new StringContent(mockResponse)
              });
            var httpClient = new HttpClient(handlerMock.Object);

            var _apiRequest = new OrchestratorApiRequest();
            var query = new PiiRecord();

            // act
            var TestQueryResult = await _apiRequest.SendQuery("http://example.com", query, httpClient);

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

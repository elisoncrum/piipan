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
                  Content = new StringContent("{\"text\":\"You did a request\"}")
              });
            var httpClient = new HttpClient(handlerMock.Object);

            var _apiRequest = new OrchestratorApiRequest();
            var query = new PiiRecord();

            // act
            var TestQueryResult = await _apiRequest.SendQuery("http://example.com", query, httpClient);

            // assert
            Assert.NotNull(TestQueryResult);
            Assert.Equal("You did a request", TestQueryResult);
            handlerMock.Protected().Verify(
              "SendAsync",
              Times.Exactly(1),
              ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
              ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}

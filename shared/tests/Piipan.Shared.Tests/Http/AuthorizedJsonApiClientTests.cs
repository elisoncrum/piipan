using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;
using Moq;
using Moq.Protected;
using Xunit;

namespace Piipan.Shared.Tests.Http
{
    public class AuthorizedJsonApiClientTests
    {
        [Fact]
        public async Task PostAsync_SendsExpectedMessage()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("response body") };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Post, "https://tts.test/path");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Post && m.RequestUri.ToString() == "https://tts.test/path"), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);
            
            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );
            var body = new StringContent("this is the message body");
            
            // Act
            var response = await apiClient.PostAsync("/path", body);

            // Assert
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("response body", responseBody);
        }

        [Fact]
        public async Task GetAsync_SendsExpectedMessage()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("response body") };

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, "https://tts.test/path");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.ToString() == "https://tts.test/path"), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://tts.test")
            };

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(m => m.CreateClient(typeof(AuthorizedJsonApiClientTests).Name))
                .Returns(client);
            
            var tokenProvider = new Mock<ITokenProvider<AuthorizedJsonApiClientTests>>();
            var apiClient = new AuthorizedJsonApiClient<AuthorizedJsonApiClientTests>(
                clientFactory.Object,
                tokenProvider.Object
            );
            
            // Act
            var response = await apiClient.GetAsync("/path");

            // Assert
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("response body", responseBody);
        }
    }
}
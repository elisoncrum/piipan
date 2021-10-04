using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Moq;
using Moq.Protected;
using Xunit;

namespace Piipan.Shared.Authentication.Tests
{

    public class AuthenticationTests
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

        static Mock<HttpMessageHandler> MockMessageHandler(HttpStatusCode status, string response)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = status,
                    Content = new StringContent(response, Encoding.UTF8, "application/json")
                })
                .Verifiable();

            return mockHttpMessageHandler;
        }

        ////
        // Tests
        ////
        [Fact]
        public async void PostAsync()
        {
            // Arrange
            var messageHandler = MockMessageHandler(HttpStatusCode.OK, "ok");
            var token = "|token|";
            var tokenProvider = MockTokenProvider(token);
            var httpClient = new HttpClient(messageHandler.Object);
            var apiClient = new AuthorizedJsonApiClient(httpClient, tokenProvider.Object);
            var uri = new Uri("https://localhost/");
            var content = new StringContent("{}");

            // Act
            var response = await apiClient.PostAsync(uri, content);
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("ok", result);
            messageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post
                    && req.RequestUri == uri
                    && req.Headers.Authorization.ToString() == $"Bearer {token}"
                ),
                ItExpr.IsAny<CancellationToken>()
            );

        }

        [Fact]
        public async void GetAsync()
        {
            // Arrange
            var token = "|token|";
            var content = "ok";
            var messageHandler = MockMessageHandler(HttpStatusCode.OK, content);
            var tokenProvider = MockTokenProvider(token);
            var httpClient = new HttpClient(messageHandler.Object);
            var apiClient = new AuthorizedJsonApiClient(httpClient, tokenProvider.Object);
            var uri = new Uri("https://localhost/");

            // Act
            var response = await apiClient.GetAsync(uri);
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(content, result);
            messageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get
                    && req.RequestUri == uri
                    && req.Headers.Authorization.ToString() == $"Bearer {token}"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }

}

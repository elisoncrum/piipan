using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;
using Moq;
using Moq.Protected;
using Xunit;
using System.Text.Json;

namespace Piipan.Shared.Tests.Http
{
    public class AuthorizedJsonApiClientTests
    {
        public class FakeRequestType
        {
            public string RequestMessage { get; set; }
        }

        public class FakeResponseType
        {
            public string ResponseMessage { get; set; }
        }

        [Fact]
        public async Task PostAsync_SendsExpectedMessage()
        {
            // Arrange
            var httpResponseContent = new FakeResponseType() { ResponseMessage = "this is a response message" };
            var json = JsonSerializer.Serialize(httpResponseContent);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

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

            var body = new FakeRequestType
            {
                RequestMessage = "this is a request message"
            };
            
            // Act
            var response = await apiClient.PostAsync<FakeRequestType, FakeResponseType>("/path", body);

            // Assert
            Assert.IsType<FakeResponseType>(response);
            Assert.Equal("this is a response message", response.ResponseMessage);
        }

        [Fact]
        public async Task PostAsync_IncludesAdditionalHeaders()
        {
            // Arrange
            var httpResponseContent = new FakeResponseType() { ResponseMessage = "this is a response message" };
            var json = JsonSerializer.Serialize(httpResponseContent);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(m => 
                        m.Method == HttpMethod.Post && 
                        m.RequestUri.ToString() == "https://tts.test/path" &&
                        m.Headers.Contains("added-header")),
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

            var body = new FakeRequestType
            {
                RequestMessage = "this is a request message"
            };
            
            // Act
            var response = await apiClient.PostAsync<FakeRequestType, FakeResponseType>("/path", body, () =>
            {
                return new List<(string, string)>
                {
                    ("added-header", "added-value")
                };
            });

            // Assert
            Assert.IsType<FakeResponseType>(response);
            Assert.Equal("this is a response message", response.ResponseMessage);
        }

        [Fact]
        public async Task GetAsync_SendsExpectedMessage()
        {
            // Arrange
            var httpResponseContent = new FakeResponseType() { ResponseMessage = "this is a response message" };
            var json = JsonSerializer.Serialize(httpResponseContent);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

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
            var response = await apiClient.GetAsync<FakeResponseType>("/path");

            // Assert
            Assert.IsType<FakeResponseType>(response);
            Assert.Equal("this is a response message", response.ResponseMessage);
        }
    }
}
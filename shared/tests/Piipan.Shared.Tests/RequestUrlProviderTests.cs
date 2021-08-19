using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Piipan.Shared.Http.Tests
{
    public class RequestUrlProviderTests
    {
        [Fact]
        public void GetBaseUrl()
        {
            // Arrange
            var provider = new RequestUrlProvider();
            var request = new Mock<HttpRequest>();
            request
                .Setup(m => m.Scheme)
                .Returns("https");
            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));
            
            // Act
            var baseUrl = provider.GetBaseUrl(request.Object);

            // Assert
            Assert.Equal("https://tts.test/", baseUrl.ToString());
        }
    }
}
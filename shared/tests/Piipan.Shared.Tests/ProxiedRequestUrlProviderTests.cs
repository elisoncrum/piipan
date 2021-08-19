using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using Moq;
using Xunit;

namespace Piipan.Shared.Http.Tests
{
    public class ProxiedRequestUrlProviderTests
    {
        [Fact]
        public void GetBaseUrl()
        {
            // Arrange
            var logger = new Mock<ILogger<ProxiedRequestUrlProvider>>();
            var provider = new ProxiedRequestUrlProvider(logger.Object);
            var request = new Mock<HttpRequest>();
            var headers = new HeaderDictionary();
            headers.Add("X-Forwarded-Proto", "https");
            headers.Add("X-Forwarded-Host", "tts.test");

            request
                .Setup(m => m.Headers)
                .Returns(headers);
            
            // Act
            var baseUrl = provider.GetBaseUrl(request.Object);

            // Assert
            Assert.Equal("https://tts.test/", baseUrl.ToString());
        }

        [Fact]
        public void GetBaseUrl_ThrowsWhenProtoHeaderMissing()
        {
            // Arrange
            var logger = new Mock<ILogger<ProxiedRequestUrlProvider>>();
            var provider = new ProxiedRequestUrlProvider(logger.Object);
            var request = new Mock<HttpRequest>();
            var headers = new HeaderDictionary();
            // no proto header
            headers.Add("X-Forwarded-Host", "tts.test");

            request
                .Setup(m => m.Headers)
                .Returns(headers);
            
            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => provider.GetBaseUrl(request.Object));

            // Assert
            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("Unable to extract")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }

        [Fact]
        public void GetBaseUrl_ThrowsWhenHostHeaderMissing()
        {
            // Arrange
            var logger = new Mock<ILogger<ProxiedRequestUrlProvider>>();
            var provider = new ProxiedRequestUrlProvider(logger.Object);
            var request = new Mock<HttpRequest>();
            var headers = new HeaderDictionary();
            headers.Add("X-Forwarded-Proto", "https");
            // no host header

            request
                .Setup(m => m.Headers)
                .Returns(headers);
            
            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => provider.GetBaseUrl(request.Object));

            // Assert
            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("Unable to extract")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }
    }
}
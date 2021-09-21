using Piipan.Metrics.Func.Collect;
using Xunit;
using Moq;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using System;

namespace Piipan.Metrics.Func.Collect.Tests
{
    public class BulkUploadMetricsTests
    {
        private EventGridEvent MockEvent(string url, DateTime eventTime)
        {
            var gridEvent = Mock.Of<EventGridEvent>();
            gridEvent.Data = new { url = url };
            gridEvent.EventTime = eventTime;
            return gridEvent;
        }

        [Fact]
        public void Run_Success()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(1);

            var function = new BulkUploadMetrics(uploadApi.Object);

            // Act
            function.Run(MockEvent("https://somethingeaupload", now), logger.Object);

            // Assert
            uploadApi.Verify(m => m.AddUpload("ea", now), Times.Once);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Number of rows inserted=1"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Theory]
        [InlineData("badurl", "State not found")] // malformed url, can't parse the state
        [InlineData("https://eupload", "State not found")] // state is only one character
        public void Run_BadUrl(string url, string expectedLogMessage)
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(1);

            var function = new BulkUploadMetrics(uploadApi.Object);

            // Act
            Assert.Throws<FormatException>(() => function.Run(MockEvent(url, now), logger.Object));

            // Assert
            uploadApi.Verify(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedLogMessage),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public void Run_UploadApiThrows()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Throws(new Exception("upload api broke"));

            var function = new BulkUploadMetrics(uploadApi.Object);

            // Act
            Assert.Throws<Exception>(() => function.Run(MockEvent("https://somethingeaupload", now), logger.Object));

            // Assert
            uploadApi.Verify(m => m.AddUpload("ea", now), Times.Once);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "upload api broke"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}
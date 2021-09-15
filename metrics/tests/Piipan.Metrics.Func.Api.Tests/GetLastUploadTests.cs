using Microsoft.AspNetCore.Http;
using Piipan.Metrics.Api;
using Piipan.Metrics.Func.Api.Builders;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Piipan.Metrics.Func.Api.Tests
{
    public class GetLastUploadTests
    {
        [Fact]
        public void Run_Success()
        {
            // Arrange
            var uploadedAt = DateTime.Now;

            var context = new DefaultHttpContext();
            var request = context.Request;

            var logger = new Mock<ILogger>();

            var uploads = new List<ParticipantUpload>
                {
                    new ParticipantUpload 
                    {
                        State = "ea",
                        UploadedAt = uploadedAt
                    }
                };
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetLatestUploadsByState())
                .Returns(uploads);

            var meta = new Meta
            {
                Page = 1,
                PerPage = 1,
                Total = 1,
                NextPage = null,
                PrevPage = null
            };
            var metaBuilder = new Mock<IMetaBuilder>();
            metaBuilder
                .Setup(m => m.Build())
                .Returns(meta);

            var function = new GetLastUpload(uploadApi.Object, metaBuilder.Object);

            // Act
            var result = function.Run(request, logger.Object) as JsonResult;
            var response = result.Value as GetParticipantUploadsResponse;
            
            // Assert
            Assert.NotNull(response);
            Assert.Equal(meta, response.Meta);
            Assert.Equal(uploads, response.Data);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing request from user")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public void Run_UploadApiThrows()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;

            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetLatestUploadsByState())
                .Throws(new Exception("upload api broke"));

            var meta = new Meta
            {
                Page = 1,
                PerPage = 1,
                Total = 1,
                NextPage = null,
                PrevPage = null
            };
            var metaBuilder = new Mock<IMetaBuilder>();
            metaBuilder
                .Setup(m => m.Build())
                .Returns(meta);

            var function = new GetLastUpload(uploadApi.Object, metaBuilder.Object);

            // Act / Assert
            Assert.Throws<Exception>(() => function.Run(request, logger.Object));
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "upload api broke"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public void Run_MetaBuilderThrows()
        {
            // Arrange
            var uploadedAt = DateTime.Now;

            var context = new DefaultHttpContext();
            var request = context.Request;

            var logger = new Mock<ILogger>();

            var uploads = new List<ParticipantUpload>
                {
                    new ParticipantUpload 
                    {
                        State = "ea",
                        UploadedAt = uploadedAt
                    }
                };
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetLatestUploadsByState())
                .Returns(uploads);

            var metaBuilder = new Mock<IMetaBuilder>();
            metaBuilder
                .Setup(m => m.Build())
                .Throws(new Exception("meta builder broke"));

            var function = new GetLastUpload(uploadApi.Object, metaBuilder.Object);

            // Act / Assert
            Assert.Throws<Exception>(() => function.Run(request, logger.Object));
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "meta builder broke"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}
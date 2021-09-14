using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using Piipan.Metrics.Func.Api.Builders;
using Piipan.Metrics.Func.Api.Extensions;
using Xunit;
using Moq;

namespace Piipan.Metrics.Func.Api.Tests
{
    public class GetParticipantUploadsTests
    {
        private Mock<IMetaBuilder> MockBuilder(Meta meta)
        {
            var metaBuilder = new Mock<IMetaBuilder>();

            metaBuilder.Setup(m => m.SetPage(It.IsAny<int>())).Returns(metaBuilder.Object);
            metaBuilder.Setup(m => m.SetPerPage(It.IsAny<int>())).Returns(metaBuilder.Object);
            metaBuilder.Setup(m => m.SetState(It.IsAny<string>())).Returns(metaBuilder.Object);
            metaBuilder
                .Setup(m => m.Build())
                .Returns(meta);

            return metaBuilder;
        }

        [Fact]
        public void Run_Success()
        {
            // Arrange
            var uploadedAt = DateTime.Now;

            var context = new DefaultHttpContext();
            var request = context.Request;
            request.QueryString = new QueryString("");

            Assert.Equal(0, request.Query.ParseInt("perPage", 0));

            var logger = new Mock<ILogger>();

            var uploads = new List<ParticipantUpload>
                {
                    new ParticipantUpload 
                    {
                        state = "ea",
                        uploaded_at = uploadedAt
                    }
                };
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploads(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(uploads);

            var meta = new Meta
            {
                page = 1,
                perPage = 1,
                total = 1,
                nextPage = null,
                prevPage = null
            };

            var function = new GetParticipantUploads(uploadApi.Object, MockBuilder(meta).Object);

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
                .Setup(m => m.GetUploads(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception("upload api broke"));

            var meta = new Meta
            {
                page = 1,
                perPage = 1,
                total = 1,
                nextPage = null,
                prevPage = null
            };

            var function = new GetParticipantUploads(uploadApi.Object, MockBuilder(meta).Object);

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
                        state = "ea",
                        uploaded_at = uploadedAt
                    }
                };
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploads(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(uploads);

            var metaBuilder = MockBuilder(null);
            metaBuilder
                .Setup(m => m.Build())
                .Throws(new Exception("meta builder broke"));

            var function = new GetParticipantUploads(uploadApi.Object, metaBuilder.Object);

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
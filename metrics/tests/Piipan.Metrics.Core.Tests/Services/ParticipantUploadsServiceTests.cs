using System;
using System.Collections.Generic;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Services;
using Moq;
using Xunit;

namespace Piipan.Metrics.Core.Tests.Services
{
    public class ParticipantUploadServiceTests
    {
        [Fact]
        public void GetUploadCount()
        {
            // Arrange
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(99);
            
            var service = new ParticipantUploadService(uploadDao.Object);
            
            // Act
            var count = service.GetUploadCount("somestate");

            // Assert
            Assert.Equal(99, count);
        }

        [Fact]
        public void GetUploads()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetUploads(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<ParticipantUpload>()
                {
                    new ParticipantUpload
                    {
                        State = "somestate",
                        UploadedAt = uploadedAt,
                    }
                });

                var service = new ParticipantUploadService(uploadDao.Object);

            // Act
            var uploads = service.GetUploads("somestate", 1, 1);

            // Assert
            Assert.Single(uploads);
            Assert.Single(uploads, (u) => u.State == "somestate");
            Assert.Single(uploads, (u) => u.UploadedAt == uploadedAt);
        }

        [Fact]
        public void GetLatestUploadsByState()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestUploadsByState())
                .Returns(new List<ParticipantUpload>()
                {
                    new ParticipantUpload
                    {
                        State = "somestate",
                        UploadedAt = uploadedAt,
                    }
                });

            var service = new ParticipantUploadService(uploadDao.Object);

            // Act
            var uploads = service.GetLatestUploadsByState();

            // Assert
            Assert.Single(uploads);
            Assert.Single(uploads, (u) => u.State == "somestate");
            Assert.Single(uploads, (u) => u.UploadedAt == uploadedAt);
        }

        [Fact]
        public void AddUpload()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(1);

            var service = new ParticipantUploadService(uploadDao.Object);

            // Act
            var nRows = service.AddUpload("somestate", uploadedAt);

            // Assert
            Assert.Equal(1, nRows);
            uploadDao.Verify(m => m.AddUpload("somestate", uploadedAt), Times.Once);
        }
    }
}
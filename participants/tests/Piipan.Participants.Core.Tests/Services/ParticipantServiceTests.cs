using System;
using System.Collections.Generic;
using System.Linq;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantServiceTests
    {
        private IEnumerable<ParticipantDbo> RandomParticipants(int n)
        {
            var result = new List<ParticipantDbo>();

            for (int i = 0; i < n; i++)
            {
                result.Add(new ParticipantDbo
                {
                    LdsHash = Guid.NewGuid().ToString(),
                    CaseId = Guid.NewGuid().ToString(),
                    ParticipantId = Guid.NewGuid().ToString(),
                    BenefitsEndDate = DateTime.UtcNow.Date,
                    RecentBenefitMonths = new List<DateTime>(),
                    ProtectLocation = (new Random()).Next(2) == 0,
                    UploadId = (new Random()).Next()
                });
            }

            return result;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetParticipants_AllMatchesReturned(int nMatches)
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participants = RandomParticipants(nMatches);
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<Int64>()))
                .ReturnsAsync(participants);
            
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestUpload())
                .ReturnsAsync(new UploadDbo
                {
                    Id = 1,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "someone"
                });

            var service = new ParticipantService(participantDao.Object, uploadDao.Object, logger);

            // Act
            var result = await service.GetParticipants("something");

            // Assert
            Assert.Equal(result, participants);
        }

        [Fact]
        public async void GetParticipants_UsesLatestUploadId()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<Int64>()))
                .ReturnsAsync(new List<ParticipantDbo>());
            
            var uploadId = (new Random()).Next();
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestUpload())
                .ReturnsAsync(new UploadDbo
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "someone"
                });

            var service = new ParticipantService(participantDao.Object, uploadDao.Object, logger);

            // Act
            var result = await service.GetParticipants("something");

            // Assert
            uploadDao.Verify(m => m.GetLatestUpload(), Times.Once);
            participantDao.Verify(m => m.GetParticipants("something", uploadId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void AddParticipants_AllAddedWithUploadId(int nParticipants)
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participants = RandomParticipants(nParticipants);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload())
                .ReturnsAsync(new UploadDbo
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "me"
                });

            var service = new ParticipantService(participantDao.Object, uploadDao.Object, logger);

            // Act
            await service.AddParticipants(participants);

            // Assert
            
            // we should add a new upload for this batch
            uploadDao.Verify(m => m.AddUpload(), Times.Once);

            // each participant added via the DAO should have the created upload ID
            participantDao
                .Verify(m => m
                    .AddParticipants(It.Is<IEnumerable<ParticipantDbo>>(participants =>
                        participants.All(p => p.UploadId == uploadId)
                    ))
                );

        }
    }
}
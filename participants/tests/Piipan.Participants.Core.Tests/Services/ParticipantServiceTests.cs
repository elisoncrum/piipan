using System;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantServiceTests
    {
        [Fact]
        public async void GetParticipant()
        {
            // Arrange
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ParticipantDbo>
                {
                    new ParticipantDbo
                    {
                        LdsHash = "lds-hash",
                        CaseId = "case-id",
                        ParticipantId = "participant-id",
                        BenefitsEndDate = DateTime.UtcNow,
                        RecentBenefitMonths = new List<DateTime>(),
                        ProtectLocation = false,
                        UploadId = 1
                    }
                });
            
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestUpload())
                .ReturnsAsync(new UploadDbo
                {
                    Id = 1,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "someone"
                });

            var service = new ParticipantService(participantDao.Object, uploadDao.Object);

            // Act
            var participants = await service.GetParticipants("something");

            // Assert
            Assert.Single(participants);
            Assert.Single(participants, p => p.ParticipantId == "participant-id");
        }
    }
}
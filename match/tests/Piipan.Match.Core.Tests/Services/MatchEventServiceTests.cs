using System.Collections.Generic;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Participants.Api.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchEventServiceTests
    {
        [Fact]
        public async void Resolve_AddsSingleRecord()
        {
            // Arrange
            var recordBuilder = new Mock<IActiveMatchRecordBuilder>();
            recordBuilder
                .Setup(r => r.SetMatch(It.IsAny<RequestPerson>(), It.IsAny<IParticipant>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.SetStates(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.GetRecord())
                .Returns(new MatchRecordDbo());

            var recordApi = new Mock<IMatchRecordApi>();
            recordApi
                .Setup(r => r.AddRecord(It.IsAny<IMatchRecord>()))
                .ReturnsAsync("foo");

            var person = new RequestPerson { LdsHash = "foo" };
            var match = new Participant { LdsHash = "foo" };
            var matches = new List<IParticipant>() { match };

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            await service.ResolveMatchesAsync(person, matches, "ea");

            // Assert
            recordApi.Verify(r => r.AddRecord(It.IsAny<IMatchRecord>()), Times.Once);
        }

        [Fact]
        public async void Resolve_AddsManyRecords()
        {
            // Arrange
            var recordBuilder = new Mock<IActiveMatchRecordBuilder>();
            recordBuilder
                .Setup(r => r.SetMatch(It.IsAny<RequestPerson>(), It.IsAny<IParticipant>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.SetStates(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.GetRecord())
                .Returns(new MatchRecordDbo());

            var recordApi = new Mock<IMatchRecordApi>();
            recordApi
                .Setup(r => r.AddRecord(It.IsAny<IMatchRecord>()))
                .ReturnsAsync("foo");

            var person = new RequestPerson { LdsHash = "foo" };
            var match = new Participant { LdsHash = "foo" };
            var matches = new List<IParticipant>() {
                match,
                match,
                match
            };

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            await service.ResolveMatchesAsync(person, matches, "ea");

            // Assert
            recordApi.Verify(r => r.AddRecord(It.IsAny<IMatchRecord>()), Times.Exactly(matches.Count));
        }
    }
}

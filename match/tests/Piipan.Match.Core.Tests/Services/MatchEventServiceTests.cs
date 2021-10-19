using System.Collections.Generic;
using System.Linq;
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
        private Mock<IActiveMatchRecordBuilder> BuilderMock(MatchRecordDbo record)
        {
            var recordBuilder = new Mock<IActiveMatchRecordBuilder>();
            recordBuilder
                .Setup(r => r.SetMatch(It.IsAny<RequestPerson>(), It.IsAny<IParticipant>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.SetStates(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.GetRecord())
                .Returns(record);

            return recordBuilder;
        }

        private Mock<IMatchRecordApi> ApiMock()
        {
            var api = new Mock<IMatchRecordApi>();
            api.Setup(r => r.AddRecord(It.IsAny<IMatchRecord>()))
                .ReturnsAsync("foo");

            return api;
        }

        [Fact]
        public async void Resolve_AddsSingleRecord()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new Participant { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<IParticipant>() { match }
            };
            response.Data.Results.Add(result);

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            await service.ResolveMatches(request, response, "ea");

            // Assert
            recordApi.Verify(r => r.AddRecord(
                It.Is<IMatchRecord>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Once);
        }

        [Fact]
        public async void Resolve_AddsManyRecords()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<IParticipant>() {
                    new Participant { LdsHash = "foo", State= "eb" },
                    new Participant { LdsHash = "foo", State = "ec" }
                }
            };
            response.Data.Results.Add(result);

            var service = new MatchEventService(recordBuilder.Object, recordApi.Object);

            // Act
            await service.ResolveMatches(request, response, "ea");

            // Assert
            recordApi.Verify(r => r.AddRecord(
                It.Is<IMatchRecord>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Exactly(result.Matches.Count()));
        }
    }
}

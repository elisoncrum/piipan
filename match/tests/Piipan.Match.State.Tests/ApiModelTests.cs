using System;
using System.Collections.Generic;
using Xunit;


namespace Piipan.Match.State.Tests
{
    public class ApiModelTests
    {
        void SetEnvironment()
        {
            Environment.SetEnvironmentVariable("StateName", "Echo Alpha");
            Environment.SetEnvironmentVariable("StateAbbr", "ea");
        }

        static PiiRecord FullRecord()
        {
            return new PiiRecord
            {
                First = "First",
                Middle = "Middle",
                Last = "Last",
                Dob = new DateTime(1970, 1, 1),
                Ssn = "000-00-0000",
                Exception = "Exception",
                CaseId = "CaseIdExample",
                ParticipantId = "ParticipantIdExample"
            };
        }

        [Fact]
        public void PiiRecordHasStateData()
        {
            // Arrange
            SetEnvironment();
            var record = FullRecord();

            // Assert
            Assert.Equal("ea", record.StateAbbr);
            Assert.Equal("Echo Alpha", record.StateName);
        }

        [Fact]
        public void PiiRecordJsonMatchesObject()
        {
            // Arrange
            SetEnvironment();
            var record = FullRecord();
            var expected = "{\n  \"last\": \"Last\",\n  \"first\": \"First\",\n  \"middle\": \"Middle\",\n  \"ssn\": \"000-00-0000\",\n  \"dob\": \"1970-01-01\",\n  \"exception\": \"Exception\",\n  \"state_name\": \"Echo Alpha\",\n  \"state_abbr\": \"ea\",\n  \"case_id\": \"CaseIdExample\",\n  \"participant_id\": \"ParticipantIdExample\"\n}";

            // Assert
            Assert.Equal(expected, record.ToJson());
        }

        [Fact]
        public void MatchResponseJsonMatchesObject()
        {
            // Arrange
            SetEnvironment();
            var record = FullRecord();
            var response = new MatchQueryResponse
            {
                Matches = new List<PiiRecord>()
            };
            var expected = "{\n  \"matches\": [\n    {" +
                    "\n      \"last\": \"Last\",\n      \"first\": \"First\",\n      \"middle\": \"Middle\",\n      \"ssn\": \"000-00-0000\",\n      \"dob\": \"1970-01-01\",\n      \"exception\": \"Exception\",\n      \"state_name\": \"Echo Alpha\",\n      \"state_abbr\": \"ea\",\n" +
                    "      \"case_id\": \"CaseIdExample\",\n      \"participant_id\": \"ParticipantIdExample\"" +
                    "\n    }\n  ]\n}";

            // Act
            response.Matches.Add(record);

            // Assert
            Assert.Equal(expected, response.ToJson());
        }
    }
}

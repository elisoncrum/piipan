using System;
using System.Collections.Generic;
using Xunit;


namespace Piipan.Match.State.Tests
{
    public class ApiModelTests
    {
        void SetEnvironment()
        {
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
                ParticipantId = "ParticipantIdExample",
                BenefitsEndMonth = new DateTime(1970, 1, 31),
                RecentBenefitMonths = new List<DateTime>() {
                  new DateTime(2021, 5, 31),
                  new DateTime(2021, 4, 30),
                  new DateTime(2021, 3, 31)
                },
                ProtectLocation = true
            };
        }

        [Fact]
        public void PiiRecordHasStateData()
        {
            // Arrange
            SetEnvironment();
            var record = FullRecord();

            // Assert
            Assert.Equal("ea", record.State);

            // Deprecated
            Assert.Equal("ea", record.StateAbbr);
        }

        [Fact]
        public void PiiRecordJsonMatchesObject()
        {
            // Arrange
            SetEnvironment();
            var record = FullRecord();
            var expected = "{\n  \"last\": \"Last\",\n  \"first\": \"First\",\n  \"middle\": \"Middle\",\n  \"ssn\": \"000-00-0000\",\n  \"dob\": \"1970-01-01\",\n  \"exception\": \"Exception\",\n  \"state\": \"ea\",\n  \"state_abbr\": \"ea\",\n  \"case_id\": \"CaseIdExample\",\n  \"participant_id\": \"ParticipantIdExample\",\n  \"benefits_end_month\": \"1970-01\",\n  \"recent_benefit_months\": [\n    \"2021-05\",\n    \"2021-04\",\n    \"2021-03\"\n  ],\n  \"protect_location\": true\n}";

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
                    "\n      \"last\": \"Last\",\n      \"first\": \"First\",\n      \"middle\": \"Middle\",\n      \"ssn\": \"000-00-0000\",\n      \"dob\": \"1970-01-01\",\n      \"exception\": \"Exception\",\n      \"state\": \"ea\",\n      \"state_abbr\": \"ea\",\n" +
                    "      \"case_id\": \"CaseIdExample\",\n      \"participant_id\": \"ParticipantIdExample\",\n      \"benefits_end_month\": \"1970-01\",\n      \"recent_benefit_months\": [\n        \"2021-05\",\n        \"2021-04\",\n        \"2021-03\"\n      ],\n      \"protect_location\": true" +
                    "\n    }\n  ]\n}";

            // Act
            response.Matches.Add(record);

            // Assert
            Assert.Equal(expected, response.ToJson());
        }
    }
}

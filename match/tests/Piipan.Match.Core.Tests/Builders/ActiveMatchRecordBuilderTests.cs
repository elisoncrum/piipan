using System;
using System.Linq;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Builders
{
    public class ActiveMatchRecordBuilderTests
    {
        [Fact]
        public void SetMatch_SetsHashAndHashType()
        {
            // Arrange
            var builder = new ActiveMatchRecordBuilder();
            var hash = "foo";
            var input = new RequestPerson
            {
                LdsHash = hash
            };
            var match = new Participant
            {
                LdsHash = hash
            };

            // Act
            var record = builder
                .SetMatch(input, match)
                .GetRecord();

            // Assert
            Assert.True(record.Hash == hash);
            Assert.True(record.HashType == "ldshash");
        }

        [Fact]
        public void SetStates_SetsInitiatingState()
        {
            // Arrange
            var builder = new ActiveMatchRecordBuilder();
            var stateA = "ea";
            var stateB = "eb";

            // Act
            var record = builder
                .SetStates(stateA, stateB)
                .GetRecord();

            // Assert
            Assert.True(record.Initiator == stateA);
        }

        [Fact]
        public void SetStates_AddsStatesAsArray()
        {
            // Arrange
            var builder = new ActiveMatchRecordBuilder();
            var states = new string[] { "ea", "eb" };

            // Act
            var record = builder
                .SetStates(states[0], states[1])
                .GetRecord();

            // Assert
            Assert.True(record.States.SequenceEqual(states));
        }

        [Fact]
        public void Builder_IsReusable()
        {
            // Arrange
            var builder = new ActiveMatchRecordBuilder();

            // Act
            var recordA = builder.GetRecord();

            // Builder should reset internal MatchRecordDbo
            // object after GetRecord() is called
            var recordB = builder.GetRecord();

            // Assert
            Assert.False(Object.ReferenceEquals(recordA, recordB));
        }

    }
}

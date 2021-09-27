using System;
using System.Collections.Generic;
using Piipan.Match.Core.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Models
{
    public class MatchRecordDboTests
    {
        [Fact]
        public void Equals_NullObj()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };

            // Act / Assert
            Assert.False(record.Equals(null));
        }

        [Fact]
        public void Equals_WrongType()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var nRecord = new
            {
                value = 1
            };

            // Act / Assert
            Assert.False(record.Equals(nRecord));
        }

        [Fact]
        public void Equals_HashCode_MatchIdMismatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var recordMismatch = new MatchRecordDbo
            {
                MatchId = record.MatchId + "1",
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Status = record.Status,
                Invalid = record.Invalid
            };


            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_InitiatorMismatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var recordMismatch = new MatchRecordDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator + "b",
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Status = record.Status,
                Invalid = record.Invalid
            };


            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_StatesMismatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var recordMismatch = new MatchRecordDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = new string[] { "a", "c" },
                Hash = record.Hash,
                HashType = record.HashType,
                Status = record.Status,
                Invalid = record.Invalid
            };


            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_HashMismatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var recordMismatch = new MatchRecordDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash + "a",
                HashType = record.HashType,
                Status = record.Status,
                Invalid = record.Invalid
            };


            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_HashTypeMismatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var recordMismatch = new MatchRecordDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType + "y",
                Status = record.Status,
                Invalid = record.Invalid
            };


            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_StatusMismatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var recordMismatch = new MatchRecordDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Status = record.Status + "t",
                Invalid = record.Invalid
            };


            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_InvalidMismatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var recordMismatch = new MatchRecordDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Status = record.Status,
                Invalid = !record.Invalid
            };


            // Act / Assert
            Assert.False(record.Equals(recordMismatch));
            Assert.NotEqual(record.GetHashCode(), recordMismatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_InputMatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Input = "{}",
                Status = "s",
                Invalid = false
            };
            var recordMatch = new MatchRecordDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Input = "{[]}",
                Status = record.Status,
                Invalid = record.Invalid
            };


            // Act / Assert
            Assert.True(record.Equals(recordMatch));
            Assert.Equal(record.GetHashCode(), recordMatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_DataMatch()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Data = "{}",
                Status = "s",
                Invalid = false
            };
            var recordMatch = new MatchRecordDbo
            {
                MatchId = record.MatchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Data = "{[]}",
                Status = record.Status,
                Invalid = record.Invalid
            };


            // Act / Assert
            Assert.True(record.Equals(recordMatch));
            Assert.Equal(record.GetHashCode(), recordMatch.GetHashCode());
        }

        [Fact]
        public void Equals_Match()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };
            var recordMatch = new MatchRecordDbo
            {
                MatchId = "m",
                Initiator = "i",
                States = new string[] { "a", "b" },
                Hash = "h",
                HashType = "t",
                Status = "s",
                Invalid = false
            };

            // Act / Assert
            Assert.True(record.Equals(recordMatch));
            Assert.Equal(record.GetHashCode(), recordMatch.GetHashCode());
        }
    }
}

using System;
using System.Collections.Generic;
using Moq;
using Piipan.Participants.Api.Models;
using Piipan.QueryTool.Extensions;
using Xunit;

#nullable enable

namespace Piipan.QueryTool.Tests.Extensions
{
    public class ParticipantExtensionsTests
    {
        [Fact]
        public void BenefitsEndDateDisplay_Null()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.BenefitsEndDate)
                .Returns<DateTime?>(null);

            // Act
            var result = participant.Object.BenefitsEndDateDisplay();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void BenefitsEndDateDisplay()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.BenefitsEndDate)
                .Returns(new DateTime(2021, 5, 31));

            // Act
            var result = participant.Object.BenefitsEndDateDisplay();

            // Assert
            Assert.Equal("2021-05", result);
        }

        [Fact]
        public void RecentBenefitMonthsDisplay_Empty()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.RecentBenefitMonths)
                .Returns(new List<DateTime>());

            // Act
            var result = participant.Object.RecentBenefitMonthsDisplay();

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void RecentBenefitMonthsDisplay_Single()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.RecentBenefitMonths)
                .Returns(new List<DateTime>
                {
                    new DateTime(2021, 5, 1)
                });

            // Act
            var result = participant.Object.RecentBenefitMonthsDisplay();

            // Assert
            Assert.Equal("2021-05", result);
        }

        [Fact]
        public void RecentBenefitMonthsDisplay_Multiple()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.RecentBenefitMonths)
                .Returns(new List<DateTime>
                {
                    new DateTime(2021, 5, 1),
                    new DateTime(2021, 4, 30),
                    new DateTime(2021, 3, 31)
                });

            // Act
            var result = participant.Object.RecentBenefitMonthsDisplay();

            // Assert
            Assert.Equal("2021-05, 2021-04, 2021-03", result);
        }

        [Theory]
        [InlineData(null, "Yes")]
        [InlineData(true, "Yes")]
        [InlineData(false, "No")]
        public void ProtectLocationDisplay(bool? protectLocation, string expected)
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.ProtectLocation)
                .Returns(protectLocation);

            // Act
            var result = participant.Object.ProtectLocationDisplay();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
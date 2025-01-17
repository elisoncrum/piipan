using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Services;
using Piipan.Participants.Api;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchServiceTests
    {
        [Fact]
        public async Task ReturnsEmptyResponseForEmptyRequest()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var requestPersonValidator = Mock.Of<IValidator<RequestPerson>>();
            var service = new MatchService(participantApi, requestPersonValidator);

            var request = new OrchMatchRequest();

            // Act
            var response = await service.FindMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Results);
            Assert.Empty(response.Data.Errors);
        }

        [Fact]
        public async Task ReturnsErrorsForInvalidPersons()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("property", "invalid value")
                }));

            var service = new MatchService(participantApi, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act
            var response = await service.FindMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Results);
            Assert.Single(response.Data.Errors);
            Assert.Single(response.Data.Errors, e => e.Index == 0);
            Assert.Single(response.Data.Errors, e => e.Detail == "invalid value");
        }

        [Fact]
        public async Task ReturnsResultsForValidPersons()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchService(participantApi, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act
            var response = await service.FindMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Errors);
            Assert.Single(response.Data.Results);
            Assert.Single(response.Data.Results, r => r.Index == 0);
        }

        [Fact]
        public async Task ReturnsAggregatedMatchesFromStates()
        {
            // Arrange
            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string> { "ea", "eb" });

            participantApi
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ParticipantMatch>
                {
                    new ParticipantMatch { ParticipantId = "p1" },
                    new ParticipantMatch { ParticipantId = "p2" }
                });

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchService(participantApi.Object, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act
            var response = await service.FindMatches(request, "ea");

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Data.Errors);
            Assert.Single(response.Data.Results);
            Assert.Single(response.Data.Results, r => r.Index == 0);

            var matches = response.Data.Results.First().Matches;
            Assert.Equal(4, matches.Count());
            Assert.Equal(2, matches.Count(m => m.ParticipantId == "p1"));
            Assert.Equal(2, matches.Count(m => m.ParticipantId == "p2"));
        }

        [Fact]
        public async Task ThrowsWhenRequestPersonValidatorThrows()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("validator failed"));

            var service = new MatchService(participantApi, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => service.FindMatches(request, "ea"));
        }

        [Fact]
        public async Task ThrowsWhenParticipantApiThrows()
        {
            // Arrange
            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string> { "ea", "eb" });
            participantApi
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("participant API failed"));

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var service = new MatchService(participantApi.Object, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => service.FindMatches(request, "ea"));
        }
    }
}

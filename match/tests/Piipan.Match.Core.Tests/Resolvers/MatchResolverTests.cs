using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Resolvers;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace Piipan.Match.Core.Tests.Resolvers
{
    public class MatchResolverTests
    {
        [Fact]
        public async Task ReturnsEmptyResponseForEmptyRequest()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var requestPersonValidator = Mock.Of<IValidator<RequestPerson>>();
            var resolver = new MatchResolver(participantApi, requestPersonValidator);

            var request = new OrchMatchRequest();

            // Act
            var response = await resolver.ResolveMatches(request);

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

            var resolver = new MatchResolver(participantApi, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act
            var response = await resolver.ResolveMatches(request);

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

            var resolver = new MatchResolver(participantApi, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act
            var response = await resolver.ResolveMatches(request);

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
                .ReturnsAsync(new List<string>{ "ea", "eb" });

            participantApi
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<IParticipant>
                {
                    new Participant { ParticipantId = "p1" },
                    new Participant { ParticipantId = "p2" }
                });
            
            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var resolver = new MatchResolver(participantApi.Object, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act
            var response = await resolver.ResolveMatches(request);

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

            var resolver = new MatchResolver(participantApi, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => resolver.ResolveMatches(request));
        }

        [Fact]
        public async Task ThrowsWhenParticipantApiThrows()
        {
            // Arrange
            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string>{ "ea", "eb" });
            participantApi
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("participant API failed"));

            var requestPersonValidator = new Mock<IValidator<RequestPerson>>();
            requestPersonValidator
                .Setup(m => m.ValidateAsync(It.IsAny<RequestPerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var resolver = new MatchResolver(participantApi.Object, requestPersonValidator.Object);

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                {
                    new RequestPerson { LdsHash = "" }
                }
            };

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => resolver.ResolveMatches(request));
        }
    }
}
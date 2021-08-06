using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.Shared.Authorization.Tests
{
    public class MinimumIdentityAssuranceLevelHandlerTests
    {

        [Fact]
        public async void HandleRequirement_Succeeds()
        {
            // Arrange
            var user = UserWithClaim("ial", "2");

            // build authorization handler
            var logger = new Mock<ILogger<MinimumIdentityAssuranceLevelHandler>>();
            var claimsProvider = new Mock<IClaimsProvider>();
            var handler = new MinimumIdentityAssuranceLevelHandler(logger.Object, claimsProvider.Object);

            // build authorization context
            var requirements = new List<IAuthorizationRequirement> {
                new MinimumIdentityAssuranceLevelRequirement(2)
            };
            var context = new AuthorizationHandlerContext(requirements, user, null);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async void HandleRequirement_FailsWhenIALBelowMinimum()
        {
            // Arrange
            var user = UserWithClaim("ial", "1");

            // build authorization handler
            var logger = new Mock<ILogger<MinimumIdentityAssuranceLevelHandler>>();
            var claimsProvider = new Mock<IClaimsProvider>();
            var handler = new MinimumIdentityAssuranceLevelHandler(logger.Object, claimsProvider.Object);

            // build authorization context
            var requirements = new List<IAuthorizationRequirement> {
                new MinimumIdentityAssuranceLevelRequirement(2)
            };
            var context = new AuthorizationHandlerContext(requirements, user, null);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
            logger.Verify(m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"below minimum")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }

        [Fact]
        public async void HandleRequirement_FailsWhenClaimMissing()
        {
            // Arrange
            var user = UserWithClaim("not-ial", "value");

            // build authorization handler
            var logger = new Mock<ILogger<MinimumIdentityAssuranceLevelHandler>>();
            var claimsProvider = new Mock<IClaimsProvider>();
            var handler = new MinimumIdentityAssuranceLevelHandler(logger.Object, claimsProvider.Object);

            // build authorization context
            var requirements = new List<IAuthorizationRequirement> {
                new MinimumIdentityAssuranceLevelRequirement(2)
            };
            var context = new AuthorizationHandlerContext(requirements, user, null);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
            logger.Verify(m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"missing ial claim")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }

        [Fact]
        public async void HandleRequirement_FailsWhenIALNotNumeric()
        {
            // Arrange
            var user = UserWithClaim("ial", "notanumber");

            // build authorization handler
            var logger = new Mock<ILogger<MinimumIdentityAssuranceLevelHandler>>();
            var claimsProvider = new Mock<IClaimsProvider>();
            var handler = new MinimumIdentityAssuranceLevelHandler(logger.Object, claimsProvider.Object);

            // build authorization context
            var requirements = new List<IAuthorizationRequirement> {
                new MinimumIdentityAssuranceLevelRequirement(2)
            };
            var context = new AuthorizationHandlerContext(requirements, user, null);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
            logger.Verify(m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Unable to convert IAL value")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }

        [Fact]
        public async void HandleRequirement_FailsWhenIALOverflowsInt32()
        {
            // Arrange
            var user = UserWithClaim("ial", "9999999999999999999999");

            // build authorization handler
            var logger = new Mock<ILogger<MinimumIdentityAssuranceLevelHandler>>();
            var claimsProvider = new Mock<IClaimsProvider>();
            var handler = new MinimumIdentityAssuranceLevelHandler(logger.Object, claimsProvider.Object);

            // build authorization context
            var requirements = new List<IAuthorizationRequirement> {
                new MinimumIdentityAssuranceLevelRequirement(2)
            };
            var context = new AuthorizationHandlerContext(requirements, user, null);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
            logger.Verify(m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"Unable to convert IAL value")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }

        private ClaimsPrincipal UserWithClaim(string type, string value)
        {
            var claims = new List<Claim> 
            {
                new Claim(type, value),
            };

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(claims));

            return claimsPrincipal;
        }
    }
}
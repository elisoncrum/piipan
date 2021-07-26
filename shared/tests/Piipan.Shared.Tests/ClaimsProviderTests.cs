using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Piipan.Shared.Claims.Tests
{
    public class ClaimsProviderTests
    {
        [Fact]
        public void GetEmail()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create<ClaimsOptions>(new ClaimsOptions {
                Email = "email_claim_type"
            });
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim("email_claim_type", "noreply@tts.test")
            }));

            // Act
            var emailClaimValue = claimsProvider.GetEmail(claimsPrincipal);

            // Assert
            Assert.Equal("noreply@tts.test", emailClaimValue);
        }

        [Fact]
        public void GetEmail_NotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create<ClaimsOptions>(new ClaimsOptions {
                Email = "email_claim_type"
            });
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim("email_claim_type_different", "noreply@tts.test")
            }));

            // Act / Assert
            Assert.Throws<System.InvalidOperationException>(() => claimsProvider.GetEmail(claimsPrincipal));

            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("Unable to extract claim with type email_claim_type for current user!")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
        }  
    }
}
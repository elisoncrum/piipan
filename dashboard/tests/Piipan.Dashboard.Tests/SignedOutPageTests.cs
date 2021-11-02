using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using Piipan.Dashboard.Pages;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class SignedOutPageTests
    {
        [Fact]
        public void Construct_CallsBasePageConstructor()
        {
            // Arrange
            var claimsProvider = new Mock<IClaimsProvider>();
            claimsProvider
                .Setup(m => m.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns("noreply@tts.test");

            // Act
            var page = new SignedOutModel(claimsProvider.Object);

            // Assert
            Assert.Equal("noreply@tts.test", page.Email);
        }
    }
}
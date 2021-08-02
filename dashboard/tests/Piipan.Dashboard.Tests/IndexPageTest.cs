using Xunit;
using Piipan.Dashboard.Pages;
using Microsoft.Extensions.Logging.Abstractions;
using Piipan.Shared.Claims;
using Moq;
using System.Security.Claims;

namespace Piipan.Dashboard.Tests
{
    public class IndexPageTests
    {
        [Fact]
        public void BeforeOnGet_MessageIsCorrect()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockClaimsProvider);

            // act

            // assert
            Assert.Equal("", pageModel.Email);
        }

        [Fact]
        public void AfterOnGet_MessageIsCorrect()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockClaimsProvider);

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        private IClaimsProvider claimsProviderMock(string email)
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            return claimsProviderMock.Object;
        }
    }
}

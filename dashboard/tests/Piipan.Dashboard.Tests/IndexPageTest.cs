using Xunit;
using Piipan.Dashboard.Pages;
using Microsoft.Extensions.Logging.Abstractions;
using Piipan.Shared.Claims;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Piipan.Dashboard.Tests
{
    public class IndexPageTests
    {
        [Fact]
        public void BeforeOnGet_EmailIsCorrect()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockClaimsProvider);
            pageModel.PageContext.HttpContext = contextMock();

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public void AfterOnGet_EmailIsCorrect()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockClaimsProvider);
            pageModel.PageContext.HttpContext = contextMock();

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        private IClaimsProvider claimsProviderMock(string email)
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            return claimsProviderMock.Object;
        }

        public static HttpContext contextMock()
        {
            var request = new Mock<HttpRequest>();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);

            return context.Object;
        }
    }
}
